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
    for (int i = 0; i < oct; i++) {
        v += a * noise3(p); t += a;
        p = p * 2.03 + vec3(1.7, 9.2, 5.3); a *= 0.5;
    }
    return v / t;
}

float warped(vec3 p) {
    vec3 q = vec3(fbm(p, 4), fbm(p + vec3(5.2, 1.3, 3.7), 4), 0.0);
    return fbm(p + 3.0 * q, 5);
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
        float lensR = body_type == 9 ? 0.12 : 0.08;
        float lensStr = lensR / max(dist, 0.03);
        vec2 lensedUV = uv * (1.0 + lensStr * smoothstep(0.4, 0.05, dist));

        // Lensed background stars
        float bg = noise3(vec3(lensedUV * 25.0, seed * 0.1));
        bg = pow(bg, 18.0) * 0.4;
        col += vec3(bg) * smoothstep(0.08, 0.25, dist);
        alpha = max(alpha, bg * 0.6);

        // Chromatic aberration
        float chrom = smoothstep(0.25, 0.06, dist);
        vec2 cOff = normalize(uv) * chrom * 0.025;
        col.r += pow(noise3(vec3((lensedUV + cOff) * 20.0, seed)), 15.0) * chrom * 0.2;
        col.b += pow(noise3(vec3((lensedUV - cOff) * 20.0, seed)), 15.0) * chrom * 0.2;
    }

    // ==================== ACCRETION DISK ====================
    if (accretion_disk) {
        vec2 dUV = uv * vec2(1.0, 3.5);
        float dDist = length(dUV);

        // Multi-arm spiral
        float sp1 = fbm(vec3(angle + dDist * 5.0 - TIME * 0.4, dDist * 8.0, seed), 6);
        float sp2 = fbm(vec3(angle * 2.0 + dDist * 3.0 - TIME * 0.3 + 3.0, dDist * 12.0, seed * 0.7), 5);

        float mask = smoothstep(0.85, 0.18, dDist) * smoothstep(0.04, 0.13, dDist);
        float density = mask * (0.3 + sp1 * 0.4 + sp2 * 0.2);

        // Temperature: blue-white → orange → red
        float tGrad = pow(smoothstep(0.7, 0.06, dDist), 2.0);
        vec3 dHot = vec3(0.7, 0.85, 1.0) * 3.0;
        vec3 dWarm = vec3(1.0, 0.7, 0.2) * 2.0;
        vec3 dCold = corona_color.rgb * 0.3;
        vec3 dCol = mix(dCold, mix(dWarm, dHot, tGrad), tGrad);

        // Doppler beaming: approaching side brighter + blue, receding dimmer + red
        float doppler = sin(angle - TIME * 0.3);
        dCol *= 1.0 + doppler * 0.3 * smoothstep(0.3, 0.08, dDist);
        dCol.b += doppler * 0.1 * tGrad;
        dCol.r -= doppler * 0.05 * tGrad;

        // Turbulence
        float turb = fbm(vec3(dUV * 8.0, TIME * 0.15 + seed), 5);
        dCol *= 0.7 + turb * 0.6;

        col += dCol * density * intensity * 0.5;
        alpha = max(alpha, density * 0.7);

        // Photon ring
        float pRing = smoothstep(0.12, 0.1, dist) * smoothstep(0.07, 0.09, dist);
        float pBright = 1.0 + fbm(vec3(angle * 8.0, TIME * 0.5, seed), 3) * 0.4;
        col += vec3(1.0, 0.95, 0.85) * pRing * 4.0 * intensity * pBright;
        alpha = max(alpha, pRing);

        // Event horizon
        float horizon = smoothstep(0.08, 0.05, dist);
        col = mix(col, vec3(0.0), horizon);
        alpha = max(alpha, horizon);

        // Relativistic jets (BH & Quasar)
        float jetAxis = abs(uv.y) / max(abs(uv.x) + 0.001, 0.001);
        float jetCore = smoothstep(3.0, 12.0, jetAxis) * smoothstep(0.8, 0.1, dist);
        float jetHalo = smoothstep(1.5, 6.0, jetAxis) * smoothstep(0.9, 0.15, dist) * 0.3;
        float jetNoise = fbm(vec3(uv * 4.0, TIME * 0.25 + seed), 4);
        float jet = (jetCore + jetHalo) * (0.6 + jetNoise * 0.4);

        // Jet knots (bright clumps moving along jet)
        float knots = pow(fbm(vec3(uv.y * 8.0 - TIME * 0.5, uv.x * 2.0, seed), 3), 3.0);
        jet += knots * jetCore * 0.5;

        vec3 jetCol = mix(corona_color.rgb, vec3(0.5, 0.6, 1.0), 0.5) * 2.0;
        // Quasar jets are more prominent
        float jetInt = body_type == 9 ? 0.35 : 0.2;
        col += jetCol * jet * intensity * jetInt;
        alpha = max(alpha, jet * 0.4);
    }

    // ==================== CORONA ====================
    if (dist < 1.0 && dist > starR * 0.8 && starR > 0.0) {
        vec3 cPos = vec3(uv * 3.0, seed);

        // Diffuse corona
        float c1 = smoothstep(1.0, starR, dist) * fbm(cPos + vec3(TIME * 0.07, 0, 0), 6) * 0.3;

        // Streamers
        float c2 = smoothstep(0.6, starR, dist) * fbm(vec3(angle * 2.0 + seed, dist * 6.0, TIME * 0.04), 5) * 0.4;

        // Prominences (arcs extending from surface)
        float prom = fbm(vec3(angle * 4.0 + seed * 2.0, TIME * 0.1, 0.0), 4);
        prom = smoothstep(0.78, 0.95, prom) * smoothstep(0.65, starR + 0.05, dist) * smoothstep(starR - 0.02, starR + 0.02, dist);

        // CME (rare expanding bubble)
        float cmePhase = flareEvent(TIME, seed);
        float cmeAngle = seed * 2.0; // fixed direction per star
        float cmeDist = smoothstep(0.7, starR + 0.1, dist) * smoothstep(starR, starR + 0.05, dist);
        float cmeAlign = pow(max(cos(angle - cmeAngle), 0.0), 4.0);
        float cme = cmeDist * cmeAlign * cmePhase * 0.6;

        float corona = (c1 + c2 + prom * 0.8 + cme) * intensity * pulse;

        // Red giant: much larger, redder corona with dust
        if (body_type == 4) {
            corona *= 1.5;
            // Dust envelope
            float dust = fbm(vec3(uv * 1.5, seed + TIME * 0.01), 5);
            float dustMask = smoothstep(0.9, starR + 0.1, dist) * 0.2;
            col += corona_color.rgb * 0.3 * dustMask * (0.5 + dust * 0.5);
            alpha = max(alpha, dustMask * 0.3);
        }

        vec3 cCol = mix(corona_color.rgb * 0.8, core_color.rgb * 1.3, smoothstep(0.5, starR, dist));
        cCol = mix(cCol, vec3(1.0, 0.95, 0.9), prom * 0.5 + cme * 0.7);

        col += cCol * corona;
        alpha = max(alpha, corona * 0.6);
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
            // RED DWARF: heavy starspots (10-40%), frequent flares
            float spots = fbm(sp * 0.4 + seed * 0.3, 5);
            float spotMask = smoothstep(0.35, 0.5, spots); // 30%+ coverage
            surfCol = mix(surfCol, surfCol * 0.25, spotMask * 0.7);

            // Frequent bright flares at random positions
            float flare = flareEvent(TIME * 2.0, seed + sp.x * 10.0);
            float flarePos = smoothstep(0.7, 0.85, fbm(sp * 2.0 + TIME * 0.5, 3));
            surfCol += vec3(1.0, 0.9, 0.7) * flare * flarePos * 2.0;

            // Granulation (smaller cells)
            float gran = voronoi(sp * 4.0 + TIME * 0.015);
            surfCol *= 0.9 + (1.0 - gran) * 0.12;

        } else if (body_type == 1) {
            // ORANGE DWARF: moderate spots, calmer
            float spots = fbm(sp * 0.5 + seed * 0.3, 5);
            surfCol = mix(surfCol, surfCol * 0.35, smoothstep(0.5, 0.62, spots) * 0.5);

            float gran = voronoi(sp * 3.5 + TIME * 0.012);
            surfCol *= 0.92 + (1.0 - gran) * 0.1;

        } else if (body_type == 2) {
            // YELLOW DWARF (Sun): granulation, spots, faculae
            float gran = voronoi(sp * 3.0 + TIME * 0.01);
            float superGran = voronoi(sp * 0.8 + TIME * 0.003);
            surfCol *= 0.88 + (1.0 - gran) * 0.15;
            surfCol *= 0.95 + smoothstep(0.0, 0.1, superGran) * 0.05;

            // Sunspots with penumbra
            float spotN = fbm(sp * 0.5 + seed * 0.3, 5);
            float penumbra = smoothstep(0.55, 0.65, spotN);
            float umbra = smoothstep(0.63, 0.72, spotN);
            surfCol = mix(surfCol, surfCol * 0.4, penumbra * 0.5);
            surfCol = mix(surfCol, vec3(0.08, 0.03, 0.01), umbra * 0.6);

            // Faculae (bright near spots)
            float fac = smoothstep(0.5, 0.55, spotN) * (1.0 - penumbra);
            surfCol = mix(surfCol, core_color.rgb * 1.25, fac * 0.2);

            // Solar flare (rare)
            float flare = flareEvent(TIME * 0.8, seed);
            float flarePos = smoothstep(0.7, 0.85, fbm(sp * 1.5 + seed * 3.0, 3));
            surfCol += vec3(1.0, 0.95, 0.85) * flare * flarePos * 1.5;

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

    // ==================== PROTOSTELLAR NEBULA EXTRAS ====================
    if (body_type == 11) {
        // Dusty envelope
        float dustEnv = fbm(vec3(uv * 1.5, seed + TIME * 0.005), 5);
        float envMask = smoothstep(0.95, 0.3, dist) * smoothstep(0.2, 0.35, dist);
        col += corona_color.rgb * 0.15 * envMask * (0.4 + dustEnv * 0.6);
        alpha = max(alpha, envMask * 0.25);

        // Bipolar outflow jets (blue-green, perpendicular to disk)
        float jetY = abs(uv.y) / max(abs(uv.x) + 0.01, 0.01);
        float jet = smoothstep(2.0, 6.0, jetY) * smoothstep(0.9, 0.2, dist);
        float jetN = fbm(vec3(uv * 3.0, TIME * 0.2 + seed), 4);
        jet *= 0.4 + jetN * 0.6;

        // HH objects (bright knots along jet)
        float knot = pow(fbm(vec3(uv.y * 6.0 - TIME * 0.3, uv.x, seed * 2.0), 3), 4.0);
        jet += knot * smoothstep(2.0, 6.0, jetY) * 0.5;

        vec3 jetCol = vec3(0.2, 0.6, 0.8);
        col += jetCol * jet * 0.3;
        alpha = max(alpha, jet * 0.3);

        // Proto-disk (thin horizontal structure)
        float diskY = abs(uv.y);
        float diskMask = smoothstep(0.08, 0.02, diskY) * smoothstep(0.15, 0.5, abs(uv.x));
        float diskN = fbm(vec3(uv.x * 5.0 + TIME * 0.05, uv.y * 20.0, seed), 4);
        col += vec3(0.5, 0.3, 0.15) * diskMask * (0.3 + diskN * 0.7) * 0.4;
        alpha = max(alpha, diskMask * 0.3);
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
