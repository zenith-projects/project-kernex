shader_type canvas_item;

// Ultra-realistic procedural celestial body shader.
// Per-type phenomena: flares, CMEs, gravitational lensing, Doppler beaming,
// convection, sunspots, pulsation, jets, dust envelopes, banded atmospheres.

uniform vec4 core_color : source_color = vec4(1.0, 0.95, 0.7, 1.0);
uniform vec4 corona_color : source_color = vec4(1.0, 0.8, 0.3, 1.0);
uniform float intensity : hint_range(0.0, 10.0) = 1.0;
uniform float pulse_speed : hint_range(0.0, 5.0) = 0.0;
uniform float seed : hint_range(0.0, 1000.0) = 0.0;
uniform bool accretion_disk = false;
uniform int body_type : hint_range(0, 11) = 2;
// 0=RedDwarf 1=OrangeDwarf 2=YellowDwarf 3=BlueGiant 4=RedGiant 5=WhiteDwarf
// 6=NeutronStar 7=Pulsar 8=BlackHole 9=Quasar 10=BrownDwarf 11=ProtostellarNebula

// ==================== 3D NOISE ====================

vec3 hash3(vec3 p) {
    p = vec3(dot(p, vec3(127.1, 311.7, 74.7)),
             dot(p, vec3(269.5, 183.3, 246.1)),
             dot(p, vec3(113.5, 271.9, 124.6)));
    return -1.0 + 2.0 * fract(sin(p) * 43758.5453);
}

float noise3(vec3 p) {
    vec3 i = floor(p), f = fract(p);
    vec3 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
    return mix(mix(mix(dot(hash3(i), f),
                       dot(hash3(i + vec3(1,0,0)), f - vec3(1,0,0)), u.x),
                   mix(dot(hash3(i + vec3(0,1,0)), f - vec3(0,1,0)),
                       dot(hash3(i + vec3(1,1,0)), f - vec3(1,1,0)), u.x), u.y),
               mix(mix(dot(hash3(i + vec3(0,0,1)), f - vec3(0,0,1)),
                       dot(hash3(i + vec3(1,0,1)), f - vec3(1,0,1)), u.x),
                   mix(dot(hash3(i + vec3(0,1,1)), f - vec3(0,1,1)),
                       dot(hash3(i + vec3(1,1,1)), f - vec3(1,1,1)), u.x), u.y), u.z) * 0.5 + 0.5;
}

float fbm(vec3 p, int oct) {
    float v = 0.0, a = 0.5, t = 0.0;
    mat3 rot = mat3(vec3(0.8, 0.6, 0.0), vec3(-0.6, 0.8, 0.0), vec3(0.0, 0.0, 1.0));
    for (int i = 0; i < oct; i++) {
        v += a * noise3(p); t += a;
        p = rot * p * 2.03 + vec3(1.7, 9.2, 5.3); a *= 0.5;
    }
    return v / t;
}

float ridged(vec3 p, int oct) {
    float v = 0.0, a = 0.5, t = 0.0, prev = 1.0;
    for (int i = 0; i < oct; i++) {
        float n = 1.0 - abs(noise3(p) * 2.0 - 1.0);
        n = n * n * prev;
        v += a * n; t += a; prev = n;
        p = p * 2.03 + vec3(1.7, 9.2, 5.3); a *= 0.5;
    }
    return v / t;
}

float warped(vec3 p) {
    vec3 q = vec3(fbm(p, 5), fbm(p + vec3(5.2, 1.3, 3.7), 5), fbm(p + vec3(9.1, 4.7, 2.8), 5));
    return fbm(p + 3.5 * q, 6);
}

float voronoi(vec3 p) {
    vec3 i = floor(p), f = fract(p);
    float d = 1.0;
    for (int x = -1; x <= 1; x++)
    for (int y = -1; y <= 1; y++)
    for (int z = -1; z <= 1; z++) {
        vec3 n = vec3(float(x), float(y), float(z));
        d = min(d, length(n + hash3(i + n) * 0.5 + 0.5 - f));
    }
    return d;
}

// Solar flare event (stochastic, seed-based timing)
float flareEvent(float time, float seedVal) {
    float t = time * 0.3 + seedVal;
    float cycle = fract(t * 0.1);
    return pow(max(sin(cycle * 6.28) * 3.0 - 2.0, 0.0), 4.0);
}

void fragment() {
    vec2 uv = UV * 2.0 - 1.0;
    float dist = length(uv);
    float angle = atan(uv.y, uv.x);

    float pulse = 1.0 + sin(TIME * pulse_speed) * 0.1 * min(pulse_speed, 1.0);
    float pulse2 = 1.0 + sin(TIME * pulse_speed * 1.7 + 2.0) * 0.04;

    vec3 col = vec3(0.0);
    float alpha = 0.0;

    // Star surface radius (varies by type)
    float starR = 0.4;
    if (body_type == 4) starR = 0.35; // red giant: larger corona region
    if (body_type == 8 || body_type == 9) starR = 0.0; // black hole/quasar: no surface

    // ==================== GRAVITATIONAL LENSING (BH/Quasar) ====================
    if (body_type == 8 || body_type == 9) {
        float schwarzschild = body_type == 9 ? 0.07 : 0.05; // event horizon size
        float einsteinR = schwarzschild * 2.6; // Einstein ring radius

        // Gravitational deflection: light bends toward the mass
        // Deflection angle ∝ 1/impact_parameter (distance from center)
        float deflection = schwarzschild * schwarzschild / max(dist * dist, 0.001);
        vec2 bendDir = normalize(uv);
        vec2 lensedUV = uv + bendDir * deflection * 0.5;

        // === Background star field (procedural, distorted by gravity) ===
        // Multiple star layers for depth
        float stars1 = noise3(vec3(lensedUV * 20.0, seed * 0.1));
        float stars2 = noise3(vec3(lensedUV * 40.0, seed * 0.3));
        float stars3 = noise3(vec3(lensedUV * 80.0, seed * 0.7));

        float starField = pow(stars1, 20.0) * 0.5 + pow(stars2, 25.0) * 0.3 + pow(stars3, 30.0) * 0.15;

        // Stars get stretched tangentially near the BH (gravitational shear)
        float shear = smoothstep(0.3, 0.08, dist);
        float tangentStretch = noise3(vec3(angle * 15.0 + dist * 20.0, seed, TIME * 0.02));
        starField += pow(tangentStretch, 12.0) * shear * 0.3;

        // Star colors (vary by brightness)
        vec3 starCol = mix(vec3(1.0, 0.9, 0.7), vec3(0.7, 0.8, 1.0), stars1);
        col += starCol * starField * smoothstep(schwarzschild, schwarzschild * 3.0, dist);
        alpha = max(alpha, starField * 0.6);

        // === Einstein ring (bright ring of lensed light) ===
        float ringDist = abs(dist - einsteinR);
        float eRing = smoothstep(0.025, 0.0, ringDist);
        // Ring is not uniform — varies with angle due to source position
        float ringVar = 0.5 + 0.5 * sin(angle * 3.0 + seed * 5.0);
        eRing *= 0.3 + ringVar * 0.7;
        col += vec3(0.9, 0.85, 0.75) * eRing * 1.5 * intensity;
        alpha = max(alpha, eRing * 0.7);

        // === Chromatic aberration (wavelength-dependent bending) ===
        float chromStr = smoothstep(0.25, 0.05, dist);
        vec2 chromOff = bendDir * chromStr * 0.03;
        // Red bends less, blue bends more (realistic dispersion)
        float chromR = pow(noise3(vec3((lensedUV - chromOff * 0.5) * 25.0, seed)), 18.0);
        float chromG = pow(noise3(vec3(lensedUV * 25.0, seed + 1.0)), 18.0);
        float chromB = pow(noise3(vec3((lensedUV + chromOff) * 25.0, seed + 2.0)), 18.0);
        col += vec3(chromR * 0.15, chromG * 0.1, chromB * 0.2) * chromStr;

        // === Space-time distortion visual (subtle warping effect) ===
        float warpVis = smoothstep(0.3, 0.06, dist) * 0.05;
        col += vec3(0.1, 0.12, 0.2) * warpVis;
    }

    // ==================== ACCRETION DISK (cinema quality) ====================
    if (accretion_disk) {
        vec2 dUV = uv * vec2(1.0, 3.5);
        float dDist = length(dUV);

        // Multi-arm spiral with turbulent structure
        float spiralBase = angle + dDist * 5.0 - TIME * 0.35;
        float sp1 = fbm(vec3(spiralBase * 2.0, dDist * 8.0, seed), 7);
        float sp2 = fbm(vec3(spiralBase * 3.0 + 3.0, dDist * 12.0, seed * 0.7), 6);
        float sp3 = fbm(vec3(spiralBase * 5.0 + 7.0, dDist * 20.0, seed * 1.3), 5); // fine detail

        // Ring structure within disk (concentric density waves)
        float rings = sin(dDist * 40.0 + sp1 * 2.0) * 0.5 + 0.5;
        rings = 0.6 + rings * 0.4;

        float mask = smoothstep(0.85, 0.15, dDist) * smoothstep(0.03, 0.12, dDist);
        float density = mask * (0.25 + sp1 * 0.35 + sp2 * 0.2 + sp3 * 0.1) * rings;

        // Temperature gradient: blue-white core → yellow → orange → red edge
        float tGrad = pow(smoothstep(0.7, 0.05, dDist), 1.8);
        vec3 dWhiteHot = vec3(0.8, 0.9, 1.0) * 3.5;
        vec3 dBlueHot = vec3(0.5, 0.7, 1.0) * 2.5;
        vec3 dYellow = vec3(1.0, 0.85, 0.4) * 2.0;
        vec3 dOrange = vec3(1.0, 0.6, 0.15) * 1.5;
        vec3 dRed = corona_color.rgb * 0.3;

        vec3 dCol = mix(dRed, dOrange, smoothstep(0.0, 0.3, tGrad));
        dCol = mix(dCol, dYellow, smoothstep(0.3, 0.6, tGrad));
        dCol = mix(dCol, dBlueHot, smoothstep(0.6, 0.85, tGrad));
        dCol = mix(dCol, dWhiteHot, smoothstep(0.85, 1.0, tGrad));

        // Doppler beaming (relativistic effect: approaching side brighter + bluer)
        float orbitalV = 0.4 / max(dDist, 0.05); // faster near center
        float doppler = sin(angle - TIME * 0.3) * min(orbitalV, 1.0);
        dCol *= 1.0 + doppler * 0.35;
        dCol.b += doppler * 0.12 * tGrad;
        dCol.r -= doppler * 0.06 * tGrad;

        // Multi-scale turbulence
        float turb1 = fbm(vec3(dUV * 6.0, TIME * 0.12 + seed), 6);
        float turb2 = fbm(vec3(dUV * 15.0, TIME * 0.2 + seed * 0.5), 4);
        dCol *= 0.65 + turb1 * 0.35 + turb2 * 0.1;

        // Hot clumps in inner disk
        float clumps = warped(vec3(dUV * 4.0, TIME * 0.1 + seed));
        float hotClump = smoothstep(0.6, 0.75, clumps) * smoothstep(0.3, 0.1, dDist);
        dCol += dWhiteHot * hotClump * 0.15;

        col += dCol * density * intensity * 0.5;
        alpha = max(alpha, density * 0.7);

        // === Photon ring (innermost stable orbit glow) ===
        float pRingInner = smoothstep(0.11, 0.09, dist) * smoothstep(0.06, 0.08, dist);
        float pRingOuter = smoothstep(0.14, 0.12, dist) * smoothstep(0.09, 0.11, dist) * 0.5;
        float pNoise = 1.0 + fbm(vec3(angle * 10.0, TIME * 0.4, seed), 4) * 0.3;
        col += vec3(1.0, 0.95, 0.85) * (pRingInner + pRingOuter) * 4.0 * intensity * pNoise;
        alpha = max(alpha, (pRingInner + pRingOuter));

        // === Event horizon (absolute black) ===
        float horizon = smoothstep(0.07, 0.04, dist);
        col = mix(col, vec3(0.0), horizon);
        alpha = max(alpha, horizon);

        // === Relativistic jets ===
        float jetWidth = body_type == 9 ? 0.08 : 0.05;
        float jetMask = smoothstep(jetWidth, jetWidth * 0.2, abs(uv.x));
        float jetExtent = smoothstep(0.12, 0.85, abs(uv.y));
        float jetBase = smoothstep(0.3, 0.12, abs(uv.y));
        float jetNoise = fbm(vec3(uv.x * 8.0, uv.y * 3.0 - TIME * 0.4 * sign(uv.y), seed), 5);
        float jetFine = fbm(vec3(uv.x * 20.0, uv.y * 8.0 - TIME * 0.6 * sign(uv.y), seed * 0.5), 4);

        float jet = jetMask * jetExtent * (0.4 + jetNoise * 0.4 + jetFine * 0.15);
        jet += jetMask * jetBase * 0.5; // brighter at base

        // Jet cone spreading
        float cone = smoothstep(0.02, 0.0, abs(uv.x) - abs(uv.y) * 0.1);
        jet += cone * smoothstep(0.12, 0.6, abs(uv.y)) * 0.15;

        // Knots moving along jet
        float knots = pow(fbm(vec3(uv.y * 6.0 - TIME * 0.5 * sign(uv.y), uv.x * 3.0, seed * 2.0), 4), 3.0);
        jet += knots * jetMask * 0.4;

        vec3 jetCol = mix(vec3(0.4, 0.55, 1.0), vec3(0.9, 0.95, 1.0), jetBase);
        float jetInt = body_type == 9 ? 0.4 : 0.25;
        col += jetCol * jet * intensity * jetInt;
        alpha = max(alpha, jet * 0.4);
    }

    // ==================== CORONA (high detail) ====================
    if (dist < 1.0 && dist > starR * 0.6 && starR > 0.0) {
        vec3 cPos = vec3(uv * 3.0, seed);
        float coronaDist = (dist - starR) / (1.0 - starR); // 0 at surface, 1 at edge

        // Layer 1: Smooth K-corona (scattered photosphere light)
        float kCorona = smoothstep(1.0, starR, dist);
        kCorona *= 0.25 / max(pow(coronaDist + 0.1, 1.5), 0.01); // 1/r^1.5 falloff (realistic)
        kCorona = min(kCorona, 2.0);

        // Layer 2: Structured F-corona (dust scattering)
        float fCorona = smoothstep(0.85, starR + 0.05, dist);
        float fNoise = fbm(cPos * 2.0 + vec3(TIME * 0.05, 0, 0), 7);
        fCorona *= fNoise * 0.3;

        // Layer 3: Coronal streamers (radial structures)
        float streamerAngle = fbm(vec3(angle * 3.0 + seed, dist * 8.0, TIME * 0.03), 6);
        float streamerAngle2 = fbm(vec3(angle * 5.0 + seed * 0.7, dist * 12.0, TIME * 0.02), 5);
        float streamers = smoothstep(0.7, starR, dist) * (streamerAngle * 0.5 + streamerAngle2 * 0.3);

        // Layer 4: Coronal loops (magnetic arcs near surface)
        float loopPhase = sin(angle * 6.0 + seed * 3.0) * 0.5 + 0.5;
        float loopDist = abs(dist - starR - 0.06 * loopPhase);
        float loops = smoothstep(0.02, 0.0, loopDist) * smoothstep(starR - 0.01, starR + 0.01, dist);
        loops *= fbm(vec3(angle * 8.0, TIME * 0.15, seed), 3);

        // Layer 5: Prominences (bright arcs extending from surface)
        float prom = fbm(vec3(angle * 4.0 + seed * 2.0, TIME * 0.08, seed * 0.5), 5);
        prom = smoothstep(0.8, 0.95, prom);
        float promShape = smoothstep(0.6, starR + 0.04, dist) * smoothstep(starR - 0.01, starR + 0.02, dist);
        prom *= promShape;

        // Layer 6: CME (coronal mass ejection — rare, dramatic)
        float cmePhase = flareEvent(TIME * 0.7, seed);
        float cmeDir = seed * 2.5;
        float cmeSpread = pow(max(cos(angle - cmeDir), 0.0), 3.0);
        float cmeFront = smoothstep(0.0, 0.15, coronaDist - cmePhase * 0.5) * smoothstep(cmePhase * 0.5 + 0.2, cmePhase * 0.5, coronaDist);
        float cme = cmeFront * cmeSpread * cmePhase * smoothstep(starR, starR + 0.05, dist);
        // CME internal structure
        float cmeNoise = fbm(vec3(angle * 5.0 + TIME * 0.3, dist * 10.0, seed * 3.0), 4);
        cme *= 0.5 + cmeNoise * 0.5;

        // Combine corona layers
        float totalCorona = (kCorona * 0.2 + fCorona + streamers + loops * 0.6 + prom * 0.8 + cme * 1.2) * intensity * pulse;
        totalCorona = min(totalCorona, 3.0);

        // Red giant: much larger corona with dust envelope
        if (body_type == 4) {
            totalCorona *= 1.8;
            float dust = warped(vec3(uv * 1.2, seed + TIME * 0.008));
            float dustMask = smoothstep(0.95, starR + 0.05, dist);
            float dustClumps = fbm(vec3(uv * 3.0, seed + TIME * 0.005), 6);
            col += corona_color.rgb * 0.25 * dustMask * (0.3 + dust * 0.4 + dustClumps * 0.3);
            alpha = max(alpha, dustMask * 0.35);
        }

        // Color: transitions from hot white near surface to cooler corona color
        vec3 cCol = mix(corona_color.rgb * 0.7, core_color.rgb * 1.4, smoothstep(0.5, starR, dist));
        cCol = mix(cCol, vec3(1.0, 0.95, 0.88), (loops + prom) * 0.4);
        cCol = mix(cCol, vec3(1.0, 0.85, 0.7), cme * 0.5);

        col += cCol * totalCorona;
        alpha = max(alpha, totalCorona * 0.5);
    }

    // ==================== STAR SURFACE ====================
    if (dist < starR + 0.02 && starR > 0.0) {
        float nD = dist / starR;
        float z = sqrt(max(0.0, 1.0 - nD * nD));
        vec3 N = normalize(vec3(uv / starR, z));

        float rA = TIME * 0.02;
        vec3 rN = vec3(N.x * cos(rA) - N.z * sin(rA), N.y, N.x * sin(rA) + N.z * cos(rA));
        vec3 sp = rN * 4.0 + seed;

        vec3 surfCol = core_color.rgb;

        // === Per-type surface ===

        if (body_type == 0) {
            // RED DWARF: heavy starspots (30-40%), frequent violent flares

            // Granulation (tighter cells than sun)
            float gran = voronoi(sp * 4.5 + TIME * 0.012);
            float granBright = pow(1.0 - gran, 0.8);
            surfCol *= 0.85 + granBright * 0.18;

            // Sub-granulation detail
            float micro = voronoi(sp * 9.0 + TIME * 0.02);
            surfCol *= 0.95 + (1.0 - micro) * 0.06;

            // Plasma turbulence
            float turb = fbm(sp * 2.5 + TIME * 0.05, 6);
            surfCol = mix(surfCol, surfCol * (0.85 + turb * 0.3), 0.3);

            // MASSIVE starspot coverage (30-40%)
            float spots1 = fbm(sp * 0.35 + seed * 0.3, 6);
            float spots2 = fbm(sp * 0.7 + seed * 0.7, 5);
            float spotMask = smoothstep(0.32, 0.48, spots1) + smoothstep(0.4, 0.55, spots2) * 0.5;
            spotMask = min(spotMask, 1.0);

            // Spot structure with filaments
            float spotFilament = ridged(sp * 3.0 + seed * 1.5, 4);
            vec3 spotCol = core_color.rgb * 0.2 * (0.7 + spotFilament * 0.3);
            surfCol = mix(surfCol, spotCol, spotMask * 0.65);

            // FREQUENT intense flares (M-dwarf flares can double star brightness)
            float flare1 = flareEvent(TIME * 2.5, seed);
            float flare2 = flareEvent(TIME * 1.8, seed * 7.0 + 3.0);
            float flarePos1 = smoothstep(0.65, 0.82, fbm(sp * 1.5 + TIME * 0.3, 4));
            float flarePos2 = smoothstep(0.7, 0.88, fbm(sp * 2.0 + seed * 5.0 + TIME * 0.4, 3));

            vec3 flareCol = vec3(1.0, 0.85, 0.6);
            surfCol += flareCol * flare1 * flarePos1 * 2.5;
            surfCol += flareCol * flare2 * flarePos2 * 1.5;

        } else if (body_type == 1) {
            // ORANGE DWARF: moderate spots, calmer
            float spots = fbm(sp * 0.5 + seed * 0.3, 5);
            surfCol = mix(surfCol, surfCol * 0.35, smoothstep(0.5, 0.62, spots) * 0.5);

            float gran = voronoi(sp * 3.5 + TIME * 0.012);
            surfCol *= 0.92 + (1.0 - gran) * 0.1;

        } else if (body_type == 2) {
            // YELLOW DWARF (Sun) — maximum detail

            // === Granulation (convection cells ~1000km) ===
            float gran = voronoi(sp * 3.5 + TIME * 0.01);
            float granBright = pow(1.0 - gran, 0.7); // bright cell centers
            float granLane = smoothstep(0.0, 0.05, gran); // dark intergranular lanes
            float granDetail = voronoi(sp * 7.0 + TIME * 0.015); // sub-granulation

            // === Supergranulation (larger convection pattern) ===
            float superGran = voronoi(sp * 0.8 + TIME * 0.003);
            float superEdge = smoothstep(0.0, 0.08, superGran);

            // === Mesogranulation (intermediate scale) ===
            float mesoGran = voronoi(sp * 1.8 + TIME * 0.006);

            // Apply granulation layers
            surfCol *= 0.82 + granBright * 0.18;
            surfCol *= 0.92 + granLane * 0.08;
            surfCol *= 0.97 + (1.0 - granDetail) * 0.04;
            surfCol *= 0.96 + superEdge * 0.04;
            surfCol *= 0.98 + (1.0 - mesoGran) * 0.03;

            // === Turbulent plasma flows ===
            float plasma = fbm(sp * 2.0 + TIME * 0.04, 7);
            float plasmaFine = fbm(sp * 5.0 + TIME * 0.06, 5);
            surfCol = mix(surfCol, surfCol * (0.9 + plasma * 0.2), 0.3);
            surfCol *= 0.96 + plasmaFine * 0.08;

            // === Sunspots (active regions) ===
            float spotN = fbm(sp * 0.4 + seed * 0.3, 6);
            float penumbra = smoothstep(0.56, 0.66, spotN);
            float umbra = smoothstep(0.64, 0.73, spotN);
            // Penumbral filaments
            float penFilament = ridged(sp * 4.0 + seed, 4);
            penumbra *= 0.6 + penFilament * 0.4;

            vec3 penumbraCol = core_color.rgb * 0.45;
            vec3 umbraCol = vec3(0.06, 0.02, 0.005);
            surfCol = mix(surfCol, penumbraCol, penumbra * 0.6);
            surfCol = mix(surfCol, umbraCol, umbra * 0.7);

            // === Faculae (bright regions near active areas) ===
            float fac = smoothstep(0.48, 0.54, spotN) * (1.0 - penumbra);
            surfCol = mix(surfCol, core_color.rgb * 1.3, fac * 0.2);

            // Faculae are brighter near limb (Wilson depression effect)
            fac *= 0.5 + 0.5 * (1.0 - z);
            surfCol += core_color.rgb * fac * 0.08;

            // === Plage (chromospheric bright regions) ===
            float plage = fbm(sp * 1.0 + seed * 0.5, 5);
            plage = smoothstep(0.55, 0.65, plage) * (1.0 - umbra);
            surfCol = mix(surfCol, core_color.rgb * 1.15, plage * 0.1);

            // === Solar flare (rare bright flash at active region) ===
            float flare = flareEvent(TIME * 0.8, seed);
            float flarePos = smoothstep(0.7, 0.85, fbm(sp * 1.5 + seed * 3.0, 4));
            surfCol += vec3(1.0, 0.97, 0.9) * flare * flarePos * 2.0;

        } else if (body_type == 3) {
            // BLUE GIANT: extremely hot, smooth with subtle plasma flows
            // Very hot = smooth but with high-energy plasma currents
            float plasma1 = fbm(sp * 1.5 + TIME * 0.06, 6);
            float plasma2 = warped(sp * 0.8 + TIME * 0.03);
            float micro = fbm(sp * 5.0, 4);

            // Almost uniform but with faint convection
            surfCol *= 0.92 + plasma1 * 0.08 + micro * 0.04;
            // Hot plasma streaks
            surfCol = mix(surfCol, surfCol * 1.15, smoothstep(0.6, 0.75, plasma2) * 0.15);

            // Stellar wind: edge brightening (UV emission)
            float windGlow = pow(1.0 - z, 2.0) * 0.15;
            surfCol += vec3(0.3, 0.4, 0.8) * windGlow;

            // Rare intense flare (blue-white flash)
            float flare = flareEvent(TIME * 0.5, seed * 2.0);
            surfCol += vec3(0.7, 0.8, 1.0) * flare * 0.8;

        } else if (body_type == 4) {
            // RED GIANT: enormous convection cells, pulsation, mass loss
            // Giant convection cells (much larger than sun's)
            float cells = voronoi(sp * 0.7 + TIME * 0.004);
            float cellBright = 1.0 - cells;
            float cellEdge = smoothstep(0.0, 0.06, cells); // dark intergranular lanes

            // Secondary smaller cells within
            float subCells = voronoi(sp * 2.0 + TIME * 0.008);
            float subBright = 1.0 - subCells;

            // Surface turbulence (slow churning)
            float turb1 = warped(sp * 0.5 + TIME * 0.006);
            float turb2 = fbm(sp * 1.5 + TIME * 0.01, 6);

            surfCol *= 0.65 + cellBright * 0.3 + subBright * 0.08;
            surfCol *= 0.9 + cellEdge * 0.1;

            // Color variation: hotter (brighter orange) vs cooler (darker red) patches
            vec3 hotPatch = core_color.rgb * 1.2;
            vec3 coolPatch = core_color.rgb * 0.6;
            surfCol = mix(surfCol, mix(coolPatch, hotPatch, turb1), 0.2);
            surfCol = mix(surfCol, corona_color.rgb * 0.7, turb2 * 0.12);

            // Pulsation: slow rhythmic brightness change
            float puls = sin(TIME * 0.12 + seed) * 0.12 + 1.0;
            float puls2 = sin(TIME * 0.07 + seed * 2.0) * 0.05 + 1.0;
            surfCol *= puls * puls2;

            // Mass loss: surface ejection patches (brighter, expanding)
            float eject = fbm(sp * 0.4 + TIME * 0.02, 4);
            surfCol += core_color.rgb * smoothstep(0.7, 0.85, eject) * 0.15;

        } else if (body_type == 5) {
            // WHITE DWARF: compact, extremely bright, smooth with subtle shimmer
            surfCol = core_color.rgb * 1.3;
            float shimmer = fbm(sp * 4.0 + TIME * 0.1, 4);
            surfCol *= 0.96 + shimmer * 0.04;

            // Faint surface magnetic pattern
            float magPattern = voronoi(sp * 3.0 + seed);
            surfCol *= 0.98 + (1.0 - magPattern) * 0.04;

            // Nova flash (rare intense brightening)
            float nova = flareEvent(TIME * 0.15, seed * 3.0);
            surfCol += vec3(0.8, 0.85, 1.0) * nova * 4.0;
            // Nova afterglow
            float afterglow = flareEvent(TIME * 0.15 - 0.3, seed * 3.0) * 0.3;
            surfCol += vec3(1.0, 0.7, 0.3) * afterglow;

        } else if (body_type == 10) {
            // BROWN DWARF: Jupiter-like banded atmosphere with storms
            float lat = rN.y;

            // Banded cloud structure
            float bandWarp = fbm(sp * vec3(0.8, 0.08, 0.8), 6) * 2.5;
            float bands = sin(lat * 10.0 + bandWarp) * 0.5 + 0.5;
            float subBands = sin(lat * 25.0 + bandWarp * 0.5) * 0.5 + 0.5;
            float fineBands = sin(lat * 50.0 + bandWarp * 0.3) * 0.5 + 0.5;

            // Zonal flow (horizontal turbulence between bands)
            float flow = fbm(sp * vec3(3.0, 0.3, 3.0) + seed, 6);
            float flowFine = fbm(sp * vec3(6.0, 0.5, 6.0), 5);

            // Methane/iron cloud patches (patchy opacity)
            float clouds = warped(sp * 1.0 + seed * 0.3);
            float cloudDetail = fbm(sp * 3.0 + seed, 5);

            // Storm vortices
            float storm = warped(sp * 0.6 + seed * 0.5);

            // Color: alternating warm and cool bands
            vec3 warm = core_color.rgb * 0.8;
            vec3 cool = corona_color.rgb * 0.6;
            surfCol = mix(cool, warm, bands * 0.6 + subBands * 0.2 + fineBands * 0.1);

            // Shear turbulence at band edges
            float bandEdge = abs(fract(lat * 5.0) - 0.5) * 2.0;
            bandEdge = 1.0 - smoothstep(0.3, 0.5, bandEdge);
            surfCol = mix(surfCol, surfCol * (0.8 + flow * 0.4), bandEdge * 0.5);
            surfCol *= 0.9 + flowFine * 0.1;

            // Cloud opacity variation (brighter = seeing through to hot interior)
            surfCol = mix(surfCol, surfCol * 0.4, smoothstep(0.55, 0.7, clouds) * 0.3);
            surfCol = mix(surfCol, surfCol * 1.3, smoothstep(0.3, 0.15, clouds) * 0.15);

            // Storm features
            surfCol = mix(surfCol, cool * 0.5, smoothstep(0.65, 0.8, storm) * 0.2);

            // Dim overall (brown dwarfs are very faint)
            surfCol *= 0.35;

        } else if (body_type == 11) {
            // PROTOSTELLAR NEBULA: young hot core with accretion glow
            float turb = warped(sp * 0.5);
            float accretionHeat = fbm(sp * 1.5 + TIME * 0.02, 5);
            surfCol = mix(core_color.rgb, corona_color.rgb, turb * 0.3);
            surfCol *= 0.7 + accretionHeat * 0.3;
            // Hotspot from accretion impact
            float hotspot = smoothstep(0.6, 0.8, fbm(sp * 2.0 + TIME * 0.05, 4));
            surfCol += core_color.rgb * hotspot * 0.3;
        }

        // Limb darkening: I = 1 - u*(1 - cosθ), u varies by type
        float limbU = body_type == 3 ? 0.3 : (body_type == 4 ? 0.8 : 0.6);
        float limb = 1.0 - limbU * (1.0 - z);
        surfCol *= limb;

        surfCol *= intensity * pulse * pulse2;

        float surfA = smoothstep(starR + 0.02, starR - 0.02, dist);
        col = mix(col, surfCol, surfA);
        alpha = max(alpha, surfA);
    }

    // ==================== NEUTRON STAR / PULSAR EFFECTS ====================
    if (body_type == 6 || body_type == 7) {
        // --- Magnetic field lines (dipole pattern) ---
        // Dipole field: curved arcs from pole to pole, like reference images
        for (int i = 0; i < 6; i++) {
            float fieldPhase = float(i) * 0.5 + seed * 0.1;
            float fieldWidth = 0.15 + float(i) * 0.12; // arcs get wider further out

            // Parametric dipole: r = fieldWidth * sin²(θ)
            // In 2D: approximate with parabolic arcs from top to bottom
            float arcCurve = uv.x * uv.x / (fieldWidth * fieldWidth);
            float arcTop = fieldWidth - arcCurve; // top arc
            float arcBot = -fieldWidth + arcCurve; // bottom arc

            float arcMaskTop = smoothstep(0.015, 0.0, abs(uv.y - arcTop));
            float arcMaskBot = smoothstep(0.015, 0.0, abs(uv.y - arcBot));
            float arcFade = smoothstep(fieldWidth * 1.2, fieldWidth * 0.3, abs(uv.x));

            float arcIntensity = (arcMaskTop + arcMaskBot) * arcFade;
            // Outer arcs dimmer
            arcIntensity *= 1.0 / (1.0 + float(i) * 0.5);

            vec3 fieldCol = mix(vec3(0.2, 0.4, 0.9), vec3(0.5, 0.7, 1.0), float(i) / 6.0);
            col += fieldCol * arcIntensity * 0.25 * intensity;
            alpha = max(alpha, arcIntensity * 0.2);
        }

        // --- Polar glow (intense emission where field lines converge) ---
        float poleGlowTop = smoothstep(0.3, 0.0, length(uv - vec2(0, 0.4)));
        float poleGlowBot = smoothstep(0.3, 0.0, length(uv + vec2(0, 0.4)));
        float poleGlow = (poleGlowTop + poleGlowBot);
        col += vec3(0.6, 0.8, 1.0) * poleGlow * 0.5 * intensity;
        alpha = max(alpha, poleGlow * 0.4);

        // --- Nebular disk / accretion spiral (like ref image 1) ---
        float spiralAngle = atan(uv.y, uv.x);
        float spiralR = dist;
        float spiral = fbm(vec3(spiralAngle * 2.0 + spiralR * 4.0 - TIME * 0.2, spiralR * 6.0, seed), 5);
        float diskMask = smoothstep(0.9, 0.3, dist) * smoothstep(0.15, 0.25, dist);
        float nebula = diskMask * spiral * 0.3;

        vec3 nebCol = mix(vec3(0.15, 0.25, 0.5), vec3(0.4, 0.6, 0.9), spiral);
        col += nebCol * nebula * intensity * 0.4;
        alpha = max(alpha, nebula * 0.3);

        // --- Polar jets (bright beams from poles) ---
        if (body_type == 7 || pulse_speed > 0.3) {
            // Jet along Y axis (vertical)
            float jetWidth = 0.06;
            float jetMask = smoothstep(jetWidth, jetWidth * 0.3, abs(uv.x));
            float jetExtent = smoothstep(0.15, 0.9, abs(uv.y));
            float jetNoise = fbm(vec3(uv.x * 10.0, uv.y * 3.0 - TIME * 0.5 * sign(uv.y), seed), 4);

            float jet = jetMask * jetExtent * (0.5 + jetNoise * 0.5);

            // Jet is brighter at base (near star)
            float jetBase = smoothstep(0.5, 0.15, abs(uv.y));
            jet += jetMask * jetBase * 0.4;

            // Jet cone spreading slightly with distance
            float coneSpread = smoothstep(0.03, 0.0, abs(uv.x) - abs(uv.y) * 0.08);
            jet += coneSpread * smoothstep(0.15, 0.7, abs(uv.y)) * 0.2;

            vec3 jetCol = mix(vec3(0.7, 0.85, 1.0), vec3(1.0, 1.0, 1.0), jetBase);
            col += jetCol * jet * 0.5 * intensity;
            alpha = max(alpha, jet * 0.5);
        }

        // --- Pulse modulation for pulsar ---
        if (body_type == 7) {
            float pulseFlash = pow(max(sin(TIME * pulse_speed * 0.5), 0.0), 4.0);
            col *= 0.8 + pulseFlash * 0.4;
            // Beam rotation
            float beamAngle = TIME * pulse_speed * 0.3;
            vec2 beamDir = vec2(cos(beamAngle), sin(beamAngle));
            float beamAlign = pow(abs(dot(normalize(uv + 0.001), beamDir)), 20.0);
            col += vec3(0.8, 0.9, 1.0) * beamAlign * smoothstep(0.8, 0.1, dist) * 0.3 * intensity;
        }

        // --- Central star glow boost ---
        float coreGlow = smoothstep(0.2, 0.0, dist);
        col += core_color.rgb * coreGlow * 0.6 * intensity;
        alpha = max(alpha, coreGlow * 0.5);
    }

    // ==================== PROTOSTELLAR NEBULA (high detail) ====================
    if (body_type == 11) {
        // === Dusty envelope (multi-layer nebula cloud) ===
        float env1 = warped(vec3(uv * 1.0, seed + TIME * 0.003));
        float env2 = fbm(vec3(uv * 2.0, seed * 0.7 + TIME * 0.004), 7);
        float env3 = fbm(vec3(uv * 4.0, seed * 1.3 + TIME * 0.002), 5);
        float envMask = smoothstep(0.98, 0.2, dist) * smoothstep(0.1, 0.25, dist);

        // Clumpy dust structure
        float dustClumps = ridged(vec3(uv * 1.5 + seed, TIME * 0.002, seed * 0.5), 6);
        float dustDensity = envMask * (env1 * 0.4 + env2 * 0.3 + dustClumps * 0.2 + env3 * 0.1);

        // Dust color: warm brown-orange near star, cold dark brown at edges
        vec3 warmDust = corona_color.rgb * 0.5;
        vec3 coldDust = corona_color.rgb * 0.15;
        vec3 dustCol = mix(coldDust, warmDust, smoothstep(0.6, 0.2, dist));
        dustCol *= 0.5 + env2 * 0.5;

        // Infrared emission glow from warm dust
        float irGlow = smoothstep(0.5, 0.2, dist) * dustDensity;
        dustCol += vec3(0.4, 0.15, 0.05) * irGlow * 0.3;

        col += dustCol * dustDensity * 0.5;
        alpha = max(alpha, dustDensity * 0.4);

        // === Circumstellar disk (protoplanetary) ===
        // Viewed slightly inclined — thin horizontal band
        float diskSqueeze = 6.0; // how thin the disk appears
        float diskR = length(vec2(uv.x, uv.y * diskSqueeze));
        float diskMask = smoothstep(0.8, 0.12, abs(uv.x)) * smoothstep(0.8, 0.08, diskR) * smoothstep(0.05, 0.1, diskR);

        // Disk structure: spiral arms, gaps, density waves
        float diskAngle = atan(uv.y * diskSqueeze, uv.x);
        float diskSpiral = fbm(vec3(diskAngle * 2.0 + diskR * 5.0 + TIME * 0.05, diskR * 10.0, seed), 6);
        float diskDetail = fbm(vec3(uv.x * 10.0 + TIME * 0.03, uv.y * 30.0, seed * 0.5), 5);
        float diskGaps = sin(diskR * 30.0 + diskSpiral * 2.0) * 0.5 + 0.5;
        diskGaps = 0.5 + diskGaps * 0.5;

        float diskDensity = diskMask * (0.3 + diskSpiral * 0.4 + diskDetail * 0.2) * diskGaps;

        // Disk color: hot inner (white-yellow), cool outer (brown-red)
        float diskTemp = smoothstep(0.5, 0.05, abs(uv.x));
        vec3 diskHot = vec3(1.0, 0.85, 0.5);
        vec3 diskWarm = vec3(0.6, 0.35, 0.12);
        vec3 diskCold = vec3(0.25, 0.12, 0.05);
        vec3 diskCol = mix(diskCold, mix(diskWarm, diskHot, diskTemp), diskTemp);
        diskCol *= 0.5 + diskDetail * 0.5;

        col += diskCol * diskDensity * 0.6;
        alpha = max(alpha, diskDensity * 0.5);

        // Disk silhouette (dark lane when seen edge-on)
        float darkLane = smoothstep(0.015, 0.005, abs(uv.y)) * smoothstep(0.1, 0.4, abs(uv.x));
        col *= 1.0 - darkLane * 0.5;

        // === Bipolar outflow jets (H-alpha emission) ===
        float jetWidth = 0.05;
        float jetMask = smoothstep(jetWidth, jetWidth * 0.15, abs(uv.x));
        float jetExtent = smoothstep(0.1, 0.85, abs(uv.y));
        float jetBase = smoothstep(0.25, 0.08, abs(uv.y));

        // Jet turbulence (different per side)
        float jetN1 = fbm(vec3(uv.x * 12.0, uv.y * 4.0 - TIME * 0.3, seed), 5);
        float jetN2 = fbm(vec3(uv.x * 8.0, uv.y * 3.0 - TIME * 0.25, seed * 2.0), 4);

        float jet = jetMask * jetExtent * (0.3 + jetN1 * 0.4 + jetN2 * 0.2);
        jet += jetMask * jetBase * 0.5; // bright at base

        // Jet cone (slightly widens with distance)
        float cone = smoothstep(0.015, 0.0, abs(uv.x) - abs(uv.y) * 0.06);
        jet += cone * smoothstep(0.1, 0.5, abs(uv.y)) * 0.15;

        // HH objects (Herbig-Haro: bright emission knots along jet)
        float hhPos1 = smoothstep(0.02, 0.0, abs(abs(uv.y) - 0.45)) * jetMask;
        float hhPos2 = smoothstep(0.02, 0.0, abs(abs(uv.y) - 0.7)) * jetMask;
        float hhNoise = fbm(vec3(uv * 5.0, seed * 3.0), 3);
        float hh = (hhPos1 + hhPos2 * 0.7) * (0.5 + hhNoise * 0.5);

        // Jet + HH color: blue-green (ionized hydrogen)
        vec3 jetCol = vec3(0.15, 0.5, 0.75);
        vec3 hhCol = vec3(0.3, 0.8, 0.6);
        col += jetCol * jet * 0.4 + hhCol * hh * 0.5;
        alpha = max(alpha, jet * 0.4 + hh * 0.4);

        // === Reflection nebula (scattered starlight in dust) ===
        float scattered = smoothstep(0.7, 0.3, dist) * env1 * 0.08;
        col += core_color.rgb * scattered;
    }

    // ==================== OUTER GLOW ====================
    if (dist > starR * 0.8 && dist < 1.0 && starR > 0.0 && !accretion_disk) {
        float glow = smoothstep(1.0, starR, dist) * 0.06 * intensity * pulse;
        col += core_color.rgb * glow;
        alpha = max(alpha, glow * 0.4);
    }

    alpha *= smoothstep(1.0, 0.92, dist);
    COLOR = vec4(col, clamp(alpha, 0.0, 1.0));
}
