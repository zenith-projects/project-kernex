shader_type canvas_item;

// Cinema-quality procedural planet shader.
// 3D noise on sphere surface — zero polar distortion.
// Each type has unique multi-layer surface with realistic detail.

uniform vec4 primary_color : source_color = vec4(0.5, 0.4, 0.3, 1.0);
uniform vec4 secondary_color : source_color = vec4(0.35, 0.3, 0.25, 1.0);
uniform vec4 atmosphere_color : source_color = vec4(0.5, 0.6, 0.9, 0.3);
uniform float atmosphere_intensity : hint_range(0.0, 1.0) = 0.2;
uniform float cloud_coverage : hint_range(0.0, 1.0) = 0.1;
uniform float rotation_speed : hint_range(0.0, 0.5) = 0.05;
uniform float seed : hint_range(0.0, 1000.0) = 0.0;
uniform vec2 light_direction = vec2(-0.5, -0.7);
uniform int planet_type : hint_range(0, 7) = 0;
uniform bool has_rings = false;
uniform vec4 ring_color : source_color = vec4(0.8, 0.75, 0.65, 0.6);
uniform float ring_tilt : hint_range(0.0, 0.8) = 0.25;
uniform float ring_scale : hint_range(0.1, 1.0) = 0.5;

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

float ridged(vec3 p, int oct) {
    float v = 0.0, a = 0.5, t = 0.0, prev = 1.0;
    for (int i = 0; i < oct; i++) {
        float n = 1.0 - abs(noise3(p) * 2.0 - 1.0);
        n = n * n * prev; // weight by previous octave for sharper ridges
        v += a * n; t += a; prev = n;
        p = p * 2.03 + vec3(1.7, 9.2, 5.3); a *= 0.5;
    }
    return v / t;
}

float warped(vec3 p) {
    vec3 q = vec3(fbm(p, 5), fbm(p + vec3(5.2, 1.3, 3.7), 5), fbm(p + vec3(9.1, 4.7, 2.8), 5));
    return fbm(p + 3.0 * q, 6);
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

// Edge distance for voronoi (for cracks)
float voronoiEdge(vec3 p) {
    vec3 i = floor(p), f = fract(p);
    float d1 = 1.0, d2 = 1.0;
    for (int x = -1; x <= 1; x++)
    for (int y = -1; y <= 1; y++)
    for (int z = -1; z <= 1; z++) {
        vec3 n = vec3(float(x), float(y), float(z));
        float d = length(n + hash3(i + n) * 0.5 + 0.5 - f);
        if (d < d1) { d2 = d1; d1 = d; } else if (d < d2) { d2 = d; }
    }
    return d2 - d1; // thin at edges, thick at centers
}

float flareEvent(float time, float seedVal) {
    float t = time * 0.3 + seedVal;
    float cycle = fract(t * 0.1);
    return pow(max(sin(cycle * 6.28) * 3.0 - 2.0, 0.0), 4.0);
}

void fragment() {
    vec2 uv = UV * 2.0 - 1.0;
    float dist = length(uv);

    // Ring system — simple elliptical projection
    vec3 ringResult = vec3(0.0);
    float ringAlpha = 0.0;
    bool ringInFront = false;

    if (has_rings) {
        // Ring is a flat disk viewed at an angle.
        // Horizontal = full width, vertical = compressed by tilt.
        // "Unsquash" Y to get the true radial distance in the ring plane.
        float squeeze = 0.2 + ring_tilt * 0.3; // how much Y is compressed (0.2 = nearly edge-on, 0.5 = tilted)
        float rr = length(vec2(uv.x, uv.y / squeeze)); // radial distance in ring plane

        // Ring band: starts outside sphere, extends outward
        float innerR = 0.52;
        float outerR = 0.52 + ring_scale * 0.42;
        float mask = smoothstep(innerR, innerR + 0.01, rr) * smoothstep(outerR, outerR - 0.01, rr);

        if (mask > 0.01) {
            // Normalized position within ring (0 = inner, 1 = outer)
            float ringPos = (rr - innerR) / max(outerR - innerR, 0.001);

            // Concentric bands + Cassini gaps
            float n1 = noise3(vec3(rr * 50.0 + seed, seed * 0.3, 0.0));
            float bands = sin(rr * 120.0 + n1 * 3.0) * 0.5 + 0.5;
            float gap1 = 1.0 - smoothstep(0.28, 0.30, ringPos) * (1.0 - smoothstep(0.32, 0.34, ringPos)); // Cassini division
            float gap2 = 1.0 - smoothstep(0.6, 0.62, ringPos) * (1.0 - smoothstep(0.64, 0.66, ringPos)); // Encke gap
            float detail = noise3(vec3(rr * 80.0, atan(uv.y, uv.x) * 4.0 + seed, seed));

            float opacity = mask * (0.3 + bands * 0.4 + detail * 0.3) * gap1 * gap2;

            // Transparency increases toward outer edge (thins out)
            opacity *= 1.0 - ringPos * 0.4;

            // Lighting
            vec3 L = normalize(vec3(light_direction, 0.6));
            float lit = max(0.2, dot(vec3(0, 1, 0), L) * 0.4 + 0.6);

            ringResult = ring_color.rgb * lit * (0.6 + detail * 0.4);
            ringAlpha = opacity * ring_color.a;

            // Front vs back: ring below equator line is in front of planet
            ringInFront = uv.y < 0.0;
        }
    }

    // Outside sphere
    if (dist > 1.0) {
        if (ringAlpha > 0.01) {
            COLOR = vec4(ringResult, ringAlpha);
        } else {
            discard;
        }
    }

    // Planet surface (only inside sphere)
    if (dist <= 1.0) {

    float z = sqrt(1.0 - dist * dist);
    vec3 N = normalize(vec3(uv, z));

    // Rotate sphere
    float a = TIME * rotation_speed;
    vec3 rN = vec3(N.x * cos(a) - N.z * sin(a), N.y, N.x * sin(a) + N.z * cos(a));
    vec3 sp = rN * 3.0 + seed;

    // Lighting
    vec3 L = normalize(vec3(light_direction, 0.6));
    vec3 V = vec3(0, 0, 1);
    vec3 H = normalize(L + V);
    float NdL = dot(N, L);
    float lat = rN.y; // -1 to 1

    vec3 col = vec3(0.0);
    float spec = 0.0, emit = 0.0;

    // ==================== ROCKY (Mars/Moon/Mercury ultra detail) ====================
    if (planet_type == 0) {
        // === Large-scale geological regions ===
        float continent = warped(sp * 0.5);
        float region = fbm(sp * 0.8, 6);

        // Maria (ancient lava plains — flat, dark, smooth)
        float mariaMask = smoothstep(0.55, 0.65, continent) * smoothstep(0.4, 0.55, region);

        // === Terrain layers ===
        float terrain = fbm(sp * 1.5, 7);
        float mtn = ridged(sp * 1.8, 7);
        float ridges = ridged(sp * 3.0 + seed * 0.5, 6);
        float detail = fbm(sp * 5.0, 6);
        float micro = fbm(sp * 10.0, 5);
        float ultraMicro = fbm(sp * 20.0, 3);

        // === Multi-scale craters ===
        float craterHuge = voronoi(sp * 0.5 + seed * 0.2);   // basin-scale
        float craterLarge = voronoi(sp * 1.2 + seed * 0.3);  // large impacts
        float craterMed = voronoi(sp * 3.0 + seed * 0.5);    // medium
        float craterSmall = voronoi(sp * 6.0 + seed * 0.8);  // small (zoom detail)

        // Crater rim brightening + floor darkening
        float hugeRim = smoothstep(0.08, 0.13, craterHuge) * (1.0 - smoothstep(0.13, 0.2, craterHuge));
        float largeRim = smoothstep(0.06, 0.1, craterLarge) * (1.0 - smoothstep(0.1, 0.16, craterLarge));
        float medRim = smoothstep(0.05, 0.08, craterMed) * (1.0 - smoothstep(0.08, 0.12, craterMed));
        float hugeFloor = 1.0 - smoothstep(0.02, 0.08, craterHuge);
        float largeFloor = 1.0 - smoothstep(0.02, 0.06, craterLarge);

        // Combined elevation
        float h = continent * 0.3 + terrain * 0.25 + mtn * 0.2 + detail * 0.1 + micro * 0.08 + ultraMicro * 0.03;
        h -= hugeFloor * 0.15 - largeFloor * 0.1; // craters lower terrain

        // === Color palette ===
        vec3 mariaCol = primary_color.rgb * 0.5;          // dark lava plains
        vec3 lowland = primary_color.rgb * 0.75;
        vec3 midland = mix(primary_color.rgb, secondary_color.rgb, 0.35);
        vec3 highland = secondary_color.rgb * 1.05;
        vec3 peak = mix(secondary_color.rgb * 1.15, vec3(0.82, 0.8, 0.77), 0.25);
        vec3 craterDark = primary_color.rgb * 0.35;
        vec3 ejecta = mix(primary_color.rgb, vec3(0.6), 0.2) * 0.9; // bright ray material

        // Elevation-based coloring
        col = mix(lowland, midland, smoothstep(0.25, 0.42, h));
        col = mix(col, highland, smoothstep(0.45, 0.62, h));
        col = mix(col, peak, smoothstep(0.7, 0.85, mtn) * 0.4);

        // Maria regions (dark smooth plains)
        col = mix(col, mariaCol, mariaMask * 0.55);

        // Crater effects
        col = mix(col, craterDark, hugeFloor * 0.25 + largeFloor * 0.15);
        col += col * (hugeRim * 0.15 + largeRim * 0.12 + medRim * 0.06);

        // Ejecta rays from large craters (bright radial streaks)
        float ejectaRay = smoothstep(0.1, 0.15, craterLarge) * smoothstep(0.2, 0.15, craterLarge);
        float rayPattern = ridged(sp * 5.0 + seed * 2.0, 3);
        col = mix(col, ejecta, ejectaRay * rayPattern * 0.12);

        // Surface roughness at all zoom levels
        col *= 0.85 + detail * 0.1 + micro * 0.06 + ultraMicro * 0.03;

        // Regional color variation (oxide deposits, mineral differences)
        float mineralVar = fbm(sp * 0.6 + seed * 1.5, 4);
        col = mix(col, col * vec3(1.05, 0.95, 0.9), mineralVar * 0.12);

        // Polar frost
        float frost = smoothstep(0.78, 0.95, abs(lat)) * fbm(sp * 5.0 + 50.0, 4);
        col = mix(col, vec3(0.86, 0.89, 0.93), frost * 0.35);
    }

    // ==================== GAS GIANT (Jupiter/Saturn ultra detail) ====================
    else if (planet_type == 1) {
        // === Multi-scale band structure ===
        // Band warp: turbulence distorts the latitude lines
        float warp1 = fbm(sp * vec3(0.8, 0.1, 0.8), 7) * 3.5;
        float warp2 = fbm(sp * vec3(0.4, 0.06, 0.4) + 20.0, 6) * 2.0;
        float warp3 = fbm(sp * vec3(1.5, 0.2, 1.5) + 40.0, 5) * 1.0;
        float latW = lat + warp1 * 0.08 + warp2 * 0.04 + warp3 * 0.02;

        // 4 band frequencies (zones = bright, belts = dark)
        float major = sin(latW * 8.0) * 0.5 + 0.5;       // 4 major zone pairs
        float secondary = sin(latW * 18.0 + 0.8) * 0.5 + 0.5; // secondary belts
        float tertiary = sin(latW * 38.0 + 1.7) * 0.5 + 0.5;  // fine bands
        float micro = sin(latW * 80.0 + 3.0) * 0.5 + 0.5;     // micro-structure (zoom detail)

        // === Zonal wind dynamics (each band has different wind speed) ===
        float zonalFlow = fbm(sp * vec3(3.0, 0.2, 3.0) + seed, 7);
        float zonalFine = fbm(sp * vec3(6.0, 0.4, 6.0) + seed * 0.5, 6);
        float zonalMicro = fbm(sp * vec3(12.0, 0.6, 12.0) + seed * 0.3, 4);

        // === Color palette ===
        vec3 zone = primary_color.rgb;                    // bright warm zones
        vec3 belt = secondary_color.rgb;                  // darker cool belts
        vec3 hotZone = primary_color.rgb * 1.15;         // brightest equatorial
        vec3 coolBelt = secondary_color.rgb * 0.6;       // darkest polar belts
        vec3 mid = mix(primary_color.rgb, secondary_color.rgb, 0.5);

        // Band color mixing
        float bandVal = major * 0.4 + secondary * 0.3 + tertiary * 0.15 + micro * 0.08;
        col = mix(coolBelt, zone, bandVal);
        col = mix(col, hotZone, smoothstep(0.7, 0.9, bandVal) * 0.2);
        col = mix(col, belt, smoothstep(0.1, 0.25, bandVal) * 0.15);

        // === Inter-band turbulence (Kelvin-Helmholtz instability) ===
        float bandEdge = abs(fract(latW * 4.0 + 0.25) - 0.5) * 2.0;
        float turbZone = 1.0 - smoothstep(0.25, 0.45, bandEdge);

        // Chevron-shaped wave patterns at band edges
        float chevron = fbm(sp * vec3(5.0, 0.3, 5.0) + warp1 * 0.5, 6);
        col = mix(col, col * (0.8 + chevron * 0.4), turbZone * 0.5);

        // Fine zonal flow streaks
        col *= 0.9 + zonalFlow * 0.08 + zonalFine * 0.04 + zonalMicro * 0.02;

        // === Storm systems (multi-scale) ===
        // Large anticyclones (Great Red Spot scale)
        float stormLarge = warped(sp * 0.5 + seed * 0.3);
        // Medium oval storms
        float stormMed = warped(sp * 1.2 + seed * 0.7 + 30.0);
        float stormMedMask = smoothstep(0.62, 0.78, stormMed);
        // Small convective storms
        float stormSmall = fbm(sp * 3.0 + seed * 1.3, 5);
        float stormSmallMask = smoothstep(0.68, 0.82, stormSmall);

        col = mix(col, coolBelt, smoothstep(0.6, 0.75, stormLarge) * 0.15);
        col = mix(col, mid, stormMedMask * 0.12);
        // Small white storms (convective plumes)
        col = mix(col, vec3(0.9, 0.88, 0.82), stormSmallMask * 0.08);

        // === Great Spot (persistent anticyclonic storm) ===
        vec3 spotPos = normalize(vec3(cos(seed * 0.1) * 0.7, 0.12, sin(seed * 0.1) * 0.7));
        float spotDist = length(rN - spotPos);
        float spotOuter = smoothstep(0.25, 0.15, spotDist);
        float spotInner = smoothstep(0.15, 0.06, spotDist);

        // Spiral structure inside the spot
        float spotAngle = atan(rN.y - spotPos.y, dot(rN - spotPos, cross(spotPos, vec3(0, 1, 0))));
        float spotSpiral = fbm(vec3(spotAngle * 3.0 + spotDist * 8.0, spotDist * 12.0, seed * 2.0), 5);

        vec3 spotEdge = mix(primary_color.rgb * 0.7, belt, 0.5);
        vec3 spotCore = primary_color.rgb * 0.45;
        vec3 spotCol = mix(spotEdge, spotCore, spotInner);
        spotCol = mix(spotCol, spotCol * (0.7 + spotSpiral * 0.6), 0.5);

        col = mix(col, spotCol, spotOuter * 0.7);

        // === Polar aurora/haze ===
        float polarDist = abs(lat);
        float polarHaze = smoothstep(0.75, 0.95, polarDist);
        float polarNoise = fbm(sp * vec3(2.0, 0.5, 2.0) + TIME * 0.005, 4);
        col = mix(col, coolBelt * 0.7, polarHaze * 0.3);
        // Aurora glow at high latitudes
        col += vec3(0.1, 0.15, 0.3) * polarHaze * polarNoise * 0.08;

        // Equatorial brightening
        col *= 0.94 + (1.0 - abs(lat)) * 0.08;
    }

    // ==================== ICE WORLD ====================
    else if (planet_type == 2) {
        // Multi-scale ice terrain
        float base = warped(sp * 0.8);
        float terrain = fbm(sp * 1.5, 7);
        float detail = fbm(sp * 4.0, 6);
        float micro = fbm(sp * 10.0, 4);

        // Tectonic crack system (Europa-style)
        float cracksMajor = voronoiEdge(sp * 1.5 + seed * 0.3);
        float cracksMinor = voronoiEdge(sp * 3.0 + seed * 0.7);
        float thinMajor = 1.0 - smoothstep(0.0, 0.03, cracksMajor);
        float wideMajor = 1.0 - smoothstep(0.0, 0.12, cracksMajor);
        float thinMinor = 1.0 - smoothstep(0.0, 0.02, cracksMinor);

        // Ice color: white-blue with subtle variation
        vec3 freshIce = vec3(0.88, 0.92, 0.98);
        vec3 oldIce = primary_color.rgb;
        vec3 dirtyIce = secondary_color.rgb * 0.8;
        col = mix(oldIce, freshIce, base * 0.4 + terrain * 0.3);
        col = mix(col, dirtyIce, detail * 0.15);
        col *= 0.9 + micro * 0.1;

        // Polar brightness
        col = mix(col, freshIce, smoothstep(0.5, 0.95, abs(lat)) * 0.3);

        // Ridges along major cracks (pressure ridges)
        col = mix(col, freshIce * 1.1, wideMajor * 0.2);

        // Subsurface ocean glow (Europa's hidden ocean)
        vec3 oceanGlow = vec3(0.08, 0.3, 0.85);
        col = mix(col, oceanGlow, thinMajor * 0.5);
        col += oceanGlow * thinMinor * 0.15;
        col += oceanGlow * wideMajor * 0.08;
        emit = thinMajor * 0.3 + thinMinor * 0.08;

        // Ice sparkle (micro-reflections)
        float sparkle = pow(fbm(sp * 15.0, 3), 3.0);
        spec = sparkle * 0.2 + thinMajor * 0.1;
    }

    // ==================== LAVA ====================
    else if (planet_type == 3) {
        // Multi-scale tectonic plate system
        float platesLarge = voronoi(sp * 1.0 + TIME * 0.002);
        float platesMed = voronoi(sp * 2.0 + TIME * 0.003);
        float edgeLarge = voronoiEdge(sp * 1.0 + TIME * 0.002);
        float edgeMed = voronoiEdge(sp * 2.0 + TIME * 0.003);

        // Crust layers
        float crustBase = fbm(sp * 2.0, 7);
        float crustDetail = fbm(sp * 5.0, 6);
        float crustMicro = fbm(sp * 12.0, 4);

        // Magma rivers: major between big plates, minor between sub-plates
        float magmaMajor = 1.0 - smoothstep(0.0, 0.04, edgeLarge);
        float magmaMinor = 1.0 - smoothstep(0.0, 0.03, edgeMed);
        float magmaGlowMajor = 1.0 - smoothstep(0.0, 0.12, edgeLarge);
        float magmaGlowMinor = 1.0 - smoothstep(0.0, 0.08, edgeMed);

        // Magma flow animation (slow movement in veins)
        float flowAnim = fbm(sp * 3.0 + vec3(TIME * 0.01, TIME * 0.008, 0), 5);

        // Volcanic hotspots (caldera-like bright spots)
        float hotspot = fbm(sp * 1.5 + TIME * 0.006, 6);
        hotspot = smoothstep(0.6, 0.72, hotspot);
        float eruption = flareEvent(TIME * 1.5, seed + sp.x * 5.0) * hotspot;

        // Cooling lava texture on plates (ridged for AA lava)
        float coolingRidges = ridged(sp * 4.0 + seed, 5);

        // Dark rocky crust with texture
        col = primary_color.rgb * (0.1 + crustBase * 0.25);
        col *= 0.85 + crustDetail * 0.15 + crustMicro * 0.05;
        // Cooling ridges on crust surface
        col = mix(col, col * 0.6, coolingRidges * 0.15);

        // Magma color: 4-step gradient from deep to white-hot
        vec3 magmaDeep = vec3(0.45, 0.02, 0.0);
        vec3 magmaMid = vec3(0.9, 0.25, 0.0);
        vec3 magmaOrange = secondary_color.rgb * 1.8;
        vec3 magmaHot = vec3(1.0, 0.85, 0.3);
        vec3 magmaWhite = vec3(1.0, 0.95, 0.7);

        float magmaTotal = min(magmaMajor + magmaMinor * 0.6, 1.0);
        float glowTotal = min(magmaGlowMajor + magmaGlowMinor * 0.4, 1.0);

        // Apply glow first (wide area warming)
        col = mix(col, magmaDeep, glowTotal * 0.4);
        // Then bright magma veins
        vec3 veinCol = mix(magmaMid, magmaOrange, flowAnim);
        col = mix(col, veinCol, magmaTotal * 0.7);
        // Hotspot centers are white-hot
        col = mix(col, magmaHot, hotspot * 0.5);
        col = mix(col, magmaWhite, eruption * 0.6);

        emit = magmaTotal * 0.5 + glowTotal * 0.15 + hotspot * 0.3 + eruption * 0.5;
    }

    // ==================== OCEAN (Earth-like) ====================
    else if (planet_type == 4) {
        // Multi-scale continent generation
        float landMask = warped(sp * 0.55 + seed * 0.1);
        float landDetail = fbm(sp * 1.5 + seed * 0.3, 6);
        float combinedLand = landMask * 0.7 + landDetail * 0.3;
        float coastLine = smoothstep(0.48, 0.54, combinedLand);

        // === OCEAN (multi-depth with currents) ===
        float oceanDepth = smoothstep(0.54, 0.25, combinedLand);
        float waves = fbm(sp * 12.0 + TIME * 0.012, 5);
        float swells = fbm(sp * 6.0 + TIME * 0.008, 4);
        float currents = warped(sp * 2.0 + vec3(TIME * 0.003, 0, 0));

        vec3 abyssal = primary_color.rgb * 0.2;   // deep trenches
        vec3 deep = primary_color.rgb * 0.4;       // deep ocean
        vec3 mid = primary_color.rgb * 0.7;        // mid depth
        vec3 shallow = primary_color.rgb + vec3(0.0, 0.12, 0.18); // shelf
        vec3 reef = primary_color.rgb * 0.9 + vec3(0.0, 0.15, 0.1); // coral reef

        vec3 ocean = mix(shallow, mid, smoothstep(0.0, 0.3, oceanDepth));
        ocean = mix(ocean, deep, smoothstep(0.3, 0.6, oceanDepth));
        ocean = mix(ocean, abyssal, smoothstep(0.7, 0.95, oceanDepth));

        // Reef near coast
        float reefZone = smoothstep(0.48, 0.5, combinedLand) * (1.0 - smoothstep(0.5, 0.52, combinedLand));
        ocean = mix(ocean, reef, reefZone * 0.5);

        // Ocean surface detail
        ocean += vec3(waves * 0.05 + swells * 0.03);
        // Current patterns (subtle color variation)
        ocean = mix(ocean, ocean * (0.9 + currents * 0.2), 0.2);

        // === LAND (detailed biomes) ===
        float elevation = fbm(sp * 2.5, 7);
        float mountains = ridged(sp * 2.0, 7);
        float riverValleys = warped(sp * 1.5 + seed * 0.5);
        float micro = fbm(sp * 8.0, 5);
        float ultraMicro = fbm(sp * 16.0, 3);

        float absLat = abs(lat);
        float moisture = 1.0 - smoothstep(0.15, 0.4, absLat) * (1.0 - elevation * 0.5);

        // Extended biome palette
        vec3 rainforest = vec3(0.04, 0.22, 0.02);
        vec3 tropical = vec3(0.08, 0.28, 0.04);
        vec3 tempForest = vec3(0.12, 0.3, 0.06);
        vec3 grassland = vec3(0.25, 0.35, 0.1);
        vec3 savanna = vec3(0.5, 0.4, 0.15);
        vec3 arid = vec3(0.55, 0.42, 0.2);
        vec3 desert = vec3(0.7, 0.6, 0.35);
        vec3 tundra = vec3(0.38, 0.4, 0.36);
        vec3 taiga = vec3(0.1, 0.2, 0.08);
        vec3 snow = vec3(0.9, 0.92, 0.96);
        vec3 rock = vec3(0.32, 0.28, 0.22);
        vec3 cliff = vec3(0.4, 0.35, 0.3);
        vec3 sand = vec3(0.78, 0.72, 0.52);
        vec3 wetland = vec3(0.15, 0.25, 0.12);

        // Latitude-based biome selection
        vec3 biome = mix(rainforest, tropical, smoothstep(0.0, 0.1, absLat));
        biome = mix(biome, tempForest, smoothstep(0.1, 0.25, absLat));
        biome = mix(biome, grassland, smoothstep(0.2, 0.35, absLat) * (1.0 - moisture * 0.5));
        biome = mix(biome, savanna, smoothstep(0.25, 0.4, absLat) * (1.0 - elevation));
        biome = mix(biome, arid, smoothstep(0.3, 0.45, absLat) * (1.0 - elevation * 0.7));
        biome = mix(biome, taiga, smoothstep(0.5, 0.65, absLat));
        biome = mix(biome, tundra, smoothstep(0.65, 0.78, absLat));
        biome = mix(biome, snow, smoothstep(0.8, 0.93, absLat));

        // Elevation modifiers
        biome = mix(biome, rock, smoothstep(0.5, 0.65, mountains));
        biome = mix(biome, cliff, smoothstep(0.6, 0.72, mountains) * 0.5);
        biome = mix(biome, snow, smoothstep(0.72, 0.85, mountains) * 0.7);

        // River valleys (greener, lower)
        float riverZone = smoothstep(0.4, 0.5, riverValleys) * (1.0 - smoothstep(0.5, 0.55, riverValleys));
        biome = mix(biome, wetland, riverZone * 0.4 * (1.0 - absLat));

        // Beach/coast transition
        float beach = smoothstep(0.48, 0.51, combinedLand) * (1.0 - smoothstep(0.51, 0.56, combinedLand));
        biome = mix(biome, sand, beach * 0.85);

        // Surface detail
        biome *= 0.88 + micro * 0.12 + ultraMicro * 0.04;

        col = mix(ocean, biome, coastLine);

        // Ice caps with variation
        float polar = abs(lat);
        float iceEdge = fbm(sp * 4.0 + 60.0, 4);
        float iceCap = smoothstep(0.78 + iceEdge * 0.08, 0.93, polar);
        col = mix(col, snow, iceCap * 0.85);

        // Ocean specular (sun glint on water)
        float sunGlint = pow(max(dot(N, H), 0.0), 120.0) * (1.0 - coastLine);
        float glintPattern = fbm(sp * 20.0 + TIME * 0.05, 3);
        spec = sunGlint * (0.3 + glintPattern * 0.3);
    }

    // ==================== DESERT ====================
    else if (planet_type == 5) {
        // Continent-scale terrain
        float continent = warped(sp * 0.5 + seed);
        // Regional terrain
        float terrain = fbm(sp * 1.2, 7);

        // Dune seas (erg) — massive dune fields with directional patterns
        float duneWind = fbm(sp * vec3(0.4, 0.08, 0.4), 4);
        float dunesLarge = ridged(sp * vec3(2.0, 0.3, 2.0) + duneWind * 3.0, 6);
        float dunesSmall = ridged(sp * vec3(5.0, 0.8, 5.0) + duneWind * 1.5, 5);
        float dunesMicro = fbm(sp * vec3(12.0, 2.0, 12.0), 4);

        // Rocky terrain (hamada — stone desert)
        float rocks = voronoi(sp * 1.2 + seed * 0.5);
        float rockTerrain = ridged(sp * 2.0 + seed, 6);
        float rockDetail = fbm(sp * 6.0, 5);

        // Canyon systems (wadi)
        float canyonEdge = voronoiEdge(sp * 0.8 + seed * 0.3);
        float canyon = 1.0 - smoothstep(0.0, 0.05, canyonEdge);

        // Mix sand vs rock regions
        float sandRegion = smoothstep(0.35, 0.55, continent);

        // Sand colors
        vec3 lightSand = primary_color.rgb * 1.15;
        vec3 deepSand = primary_color.rgb * 0.85;
        vec3 goldSand = mix(primary_color.rgb, vec3(1.0, 0.85, 0.5), 0.2);
        vec3 rockCol = secondary_color.rgb * 0.65;
        vec3 darkRock = secondary_color.rgb * 0.35;

        // Sand surface
        vec3 sandCol = mix(deepSand, lightSand, terrain * 0.4);
        sandCol = mix(sandCol, goldSand, dunesLarge * 0.2);
        // Dune self-shadowing (dark troughs between crests)
        sandCol *= 0.8 + dunesLarge * 0.15 + dunesSmall * 0.08;
        sandCol *= 0.95 + dunesMicro * 0.1;

        // Rock surface
        vec3 rockSurf = mix(rockCol, darkRock, rockTerrain * 0.3);
        rockSurf *= 0.85 + rockDetail * 0.15;

        col = mix(rockSurf, sandCol, sandRegion);

        // Canyons cut through everything
        col = mix(col, darkRock, canyon * 0.5);

        // Polar frost dusting
        float polar = smoothstep(0.75, 0.95, abs(lat));
        float frostNoise = fbm(sp * 5.0 + 80.0, 3);
        col = mix(col, vec3(0.8, 0.78, 0.72), polar * frostNoise * 0.25);
    }

    // ==================== TOXIC (Venus-like) ====================
    else if (planet_type == 6) {
        // === Multi-altitude atmospheric layers ===

        // Upper haze (high altitude, fast moving)
        float upperHaze = warped(sp * 0.4 + vec3(TIME * 0.008, TIME * 0.003, 0));
        float upperDetail = fbm(sp * 1.5 + vec3(TIME * 0.006, 0, 0), 7);

        // Mid cloud deck (main visible layer, sulfuric acid clouds)
        float midCloud1 = warped(sp * 0.6 + vec3(TIME * 0.005, 0, 0));
        float midCloud2 = warped(sp * 0.9 + vec3(0, TIME * 0.004, 0) + 30.0);
        float midDetail = fbm(sp * 2.5 + TIME * 0.003, 6);

        // Lower atmosphere (denser, slower, glimpses of surface)
        float lowerAtmo = fbm(sp * 1.0 + vec3(TIME * 0.002, 0, 0), 6);
        float lowerDetail = fbm(sp * 3.5 + seed, 5);

        // === Atmospheric dynamics ===

        // Super-rotation (upper atmosphere moves faster than surface)
        float superRot = fbm(sp * vec3(1.5, 0.2, 1.5) + vec3(TIME * 0.01, 0, 0), 5);

        // Vortex/cyclone patterns
        float vortex1 = warped(sp * 0.7 + midCloud1 * 1.5);
        float vortex2 = fbm(sp * vec3(2.0, 0.4, 2.0) + superRot * 2.0, 5);

        // Y-shaped UV absorber pattern (discovered on Venus by UV imaging)
        float yPattern = sin(lat * 3.14159 * 2.0 + superRot * 3.0) * 0.5 + 0.5;
        yPattern *= fbm(sp * vec3(1.0, 0.15, 1.0), 4);

        // === Color composition ===
        vec3 upperCol = mix(primary_color.rgb * 0.9, secondary_color.rgb, upperHaze * 0.4);
        vec3 midCol = mix(primary_color.rgb, secondary_color.rgb, midCloud1 * 0.5 + midCloud2 * 0.3);
        vec3 lowerCol = primary_color.rgb * 0.7;

        // Layer blend based on depth (upper on top, lower glimpsed through gaps)
        float upperOpacity = 0.3 + upperHaze * 0.3 + upperDetail * 0.1;
        col = lowerCol;
        col = mix(col, midCol, 0.5 + midDetail * 0.3);
        col = mix(col, upperCol, upperOpacity * 0.4);

        // Vortex dark/bright bands
        col = mix(col, col * (0.8 + vortex1 * 0.4), 0.3);
        col = mix(col, col * 0.7, smoothstep(0.6, 0.75, vortex2) * 0.15);

        // Y-pattern (UV absorber — darker bands)
        col = mix(col, col * 0.75, yPattern * 0.15);

        // Chemical streaks (bright acid reflections)
        float streaks = ridged(sp * vec3(2.5, 0.3, 2.5) + seed, 6);
        col = mix(col, secondary_color.rgb * 1.6, smoothstep(0.65, 0.82, streaks) * 0.15);

        // Fine turbulence visible in close zoom
        float fineTurb = fbm(sp * 5.0 + TIME * 0.004, 5);
        col *= 0.92 + fineTurb * 0.08;

        // === Surface thermal glow (visible through thin spots) ===
        float surfaceGlimpse = fbm(sp * 2.0 + seed, 6);
        float thinSpot = smoothstep(0.7, 0.85, surfaceGlimpse);
        // Hot surface radiates orange-red through the atmosphere
        col += vec3(0.8, 0.3, 0.05) * thinSpot * 0.08;
        emit = thinSpot * 0.05;

        // === Polar double vortex (Venus has twin cyclones at south pole) ===
        float polarDist = abs(lat) / 1.0;
        float polarVortex = smoothstep(0.65, 0.95, polarDist);
        // Twin eye pattern
        float twinEye = sin(atan(rN.x, rN.z) * 2.0 + seed) * 0.5 + 0.5;
        float pvSwirl = warped(sp * 2.0 + vec3(TIME * 0.008, 0, 0));
        float pvDetail = fbm(sp * 4.0 + vec3(TIME * 0.01, 0, 0), 5);
        col = mix(col, col * (0.6 + pvSwirl * 0.5 + pvDetail * 0.15), polarVortex * 0.4);
        col = mix(col, col * 0.5, polarVortex * twinEye * 0.2);
    }

    // ==================== CRYSTALLINE ====================
    else if (planet_type == 7) {
        // Multi-scale crystal structure
        float facetsLarge = voronoi(sp * 1.5 + seed);
        float facetsMed = voronoi(sp * 3.0 + seed * 0.7);
        float edgesLarge = voronoiEdge(sp * 1.5 + seed);
        float edgesMed = voronoiEdge(sp * 3.0 + seed * 0.7);

        float thinEdge = 1.0 - smoothstep(0.0, 0.025, edgesLarge);
        float medEdge = 1.0 - smoothstep(0.0, 0.06, edgesLarge);
        float fineEdge = 1.0 - smoothstep(0.0, 0.03, edgesMed);

        // Deep internal crystal matrix
        float inner = warped(sp * 1.0 + seed * 0.5);
        float deepInner = fbm(sp * 2.5 + seed, 7);
        float ultraDeep = fbm(sp * 5.0 + seed * 1.3, 5);

        // Prismatic color: changes with viewing angle (like real crystals)
        float viewAngle = dot(rN, vec3(0.3, 0.5, 0.7));
        float hue1 = inner * 0.4 + viewAngle * 0.6;
        float hue2 = deepInner * 0.5 + viewAngle * 0.3 + 0.33;
        vec3 prism1 = vec3(
            0.5 + 0.5 * sin(hue1 * 6.28),
            0.5 + 0.5 * sin(hue1 * 6.28 + 2.09),
            0.5 + 0.5 * sin(hue1 * 6.28 + 4.18));
        vec3 prism2 = vec3(
            0.5 + 0.5 * sin(hue2 * 6.28 + 1.0),
            0.5 + 0.5 * sin(hue2 * 6.28 + 3.09),
            0.5 + 0.5 * sin(hue2 * 6.28 + 5.18));

        // Base crystal body
        col = mix(primary_color.rgb * 0.6, secondary_color.rgb * 0.8, facetsLarge * 0.4 + facetsMed * 0.2);

        // Prismatic refraction (double layer for depth)
        col = mix(col, prism1 * 0.7, 0.25 + medEdge * 0.15);
        col = mix(col, prism2 * 0.6, fineEdge * 0.15);

        // Crystal depth: darker in thick areas, transparent at edges
        float crystalDepth = 1.0 - facetsLarge * 0.5;
        col *= 0.5 + crystalDepth * 0.5;

        // Edge highlights (light catching crystal facet boundaries)
        float edgeLight = thinEdge * pow(max(dot(N, L), 0.0), 0.5);
        float fineLight = fineEdge * pow(max(dot(N, H), 0.0), 2.0);
        col += vec3(0.9, 0.95, 1.0) * edgeLight * 0.5;
        col += prism1 * fineLight * 0.2;

        // Internal luminescence (crystal glows from within)
        float glow = smoothstep(0.3, 0.65, deepInner) * (1.0 - facetsLarge * 0.5);
        float deepGlow = smoothstep(0.5, 0.8, ultraDeep) * 0.5;
        col += mix(prism1, prism2, 0.5) * (glow * 0.2 + deepGlow * 0.1);

        // Surface sparkle (facet reflections catching light)
        float sparkle = pow(max(1.0 - facetsLarge * 2.5, 0.0), 15.0);
        float sparkleAngle = pow(max(dot(N, H), 0.0), 120.0);
        float microSparkle = pow(max(1.0 - facetsMed * 3.0, 0.0), 20.0) * pow(max(dot(N, H), 0.0), 60.0);

        spec = sparkle * sparkleAngle * 0.9 + microSparkle * 0.3;
        emit = glow * 0.15 + deepGlow * 0.08 + sparkle * 0.04;
    }

    // ==================== PBR-STYLE LIGHTING ====================

    // Diffuse: energy-conserving wrapped Lambert with softer terminator
    float wrap = 0.15; // controls how far light wraps around
    float diff = max(0.0, (NdL + wrap) / (1.0 + wrap));
    diff = pow(diff, 1.2);

    // Fresnel-weighted specular (Schlick approximation)
    float fresnel = pow(1.0 - max(dot(N, V), 0.0), 5.0);
    float specPower = 60.0 + fresnel * 40.0;
    float specHighlight = pow(max(dot(N, H), 0.0), specPower) * (0.15 + fresnel * 0.3) + spec;

    // Ambient: indirect light from atmosphere bounce
    float ambientOcclusion = 0.7 + 0.3 * z; // AO at edges
    vec3 ambient = mix(vec3(0.008), atmosphere_color.rgb * 0.05, atmosphere_intensity);
    ambient *= ambientOcclusion;

    // Subsurface scattering on terminator (light passing through thin atmosphere)
    float sss = pow(max(0.0, -NdL + 0.3), 2.0) * 0.15 * atmosphere_intensity;
    vec3 sssColor = atmosphere_color.rgb * 0.8;

    col = col * (ambient + diff * 0.92) + vec3(specHighlight) + col * emit + sssColor * sss;

    // ==================== CLOUDS (multi-layer) ====================

    if (cloud_coverage > 0.01) {
        // High altitude cirrus
        vec3 cspHigh = rN * 2.2 + seed + vec3(TIME * 0.005, 0, 0);
        float cirrus = fbm(cspHigh * 4.0 + 200.0, 4);
        cirrus = smoothstep(0.6, 0.85, cirrus) * 0.3;

        // Mid-level cumulus (main cloud layer)
        vec3 cspMid = rN * 2.5 + seed + vec3(TIME * 0.004, 0, 0);
        float cb = warped(cspMid);
        float cd = fbm(cspMid * 3.0 + 80.0, 6);
        float cumulus = cb * 0.6 + cd * 0.4;
        cumulus = smoothstep(1.0 - cloud_coverage, 1.0 - cloud_coverage * 0.08, cumulus);

        // Low-level stratus wisps
        vec3 cspLow = rN * 2.8 + seed + vec3(TIME * 0.003, seed * 0.1, 0);
        float stratus = fbm(cspLow * 5.0 + 300.0, 4);
        stratus = smoothstep(0.55, 0.75, stratus) * 0.2;

        float totalClouds = min(cumulus + cirrus + stratus, 1.0);

        // Cloud shadow on surface (displaced slightly toward light)
        col *= 1.0 - totalClouds * 0.12;

        // Cloud lighting: lit side bright, dark side dim, edges glow
        float cLit = max(0.0, NdL * 0.6 + 0.4);
        float cloudEdge = max(0.0, cumulus - 0.3) * 3.0; // thicker = brighter top
        vec3 cloudBright = mix(vec3(0.85), vec3(1.0, 0.98, 0.95), cLit);
        vec3 cloudDark = atmosphere_color.rgb * 0.3;
        vec3 cloudCol = mix(cloudDark, cloudBright, cLit * 0.7 + cloudEdge * 0.3);

        // Sunset/sunrise coloring on clouds near terminator
        float termDist = abs(NdL);
        vec3 sunsetTint = vec3(1.0, 0.6, 0.3);
        cloudCol = mix(cloudCol, sunsetTint, smoothstep(0.2, 0.0, termDist) * 0.25);

        col = mix(col, cloudCol, totalClouds * 0.85);

        // Cirrus is semi-transparent and bright
        col = mix(col, vec3(1.0, 0.98, 0.96) * cLit, cirrus * 0.3);
    }

    // ==================== ATMOSPHERE (multi-scatter) ====================

    float rim = 1.0 - z;
    float litSide = max(0.0, NdL * 0.7 + 0.3);
    vec3 scatter = atmosphere_color.rgb;

    // Primary Rayleigh scattering (blue glow on lit side)
    vec3 rayleigh = scatter * 1.8 * pow(rim, 2.5) * atmosphere_intensity * 3.0 * litSide;

    // Mie scattering (forward scatter halo near light source)
    float mieAngle = max(dot(N, L), 0.0);
    float mie = pow(mieAngle, 8.0) * pow(rim, 2.0) * atmosphere_intensity * 0.8;
    vec3 mieColor = mix(scatter, vec3(1.0, 0.9, 0.8), 0.5);

    // Terminator glow (light bending through thick atmosphere at edge)
    float terminator = 1.0 - smoothstep(-0.15, 0.25, NdL);
    vec3 termGlow = scatter * pow(rim, 1.5) * terminator * atmosphere_intensity * 0.5;

    // Night side atmospheric glow (reflected starlight, airglow)
    float nightRim = smoothstep(0.0, -0.2, NdL) * rim;
    vec3 nightGlow = scatter * 0.15 * nightRim + secondary_color.rgb * 0.03 * nightRim;

    // Outer atmospheric haze (visible even slightly beyond sphere edge)
    col += rayleigh + mieColor * mie + termGlow + nightGlow;

    // Ring in front of planet (bottom half passes over sphere)
    if (has_rings && ringAlpha > 0.0 && ringInFront) {
        col *= 1.0 - ringAlpha * 0.2; // shadow
        col = mix(col, ringResult, ringAlpha * 0.85); // overlay
    }
    // Ring shadow on planet from back ring (top half casts shadow)
    if (has_rings && ringAlpha > 0.0 && !ringInFront && dist < 0.99) {
        col *= 1.0 - ringAlpha * 0.15;
    }

    COLOR = vec4(col, smoothstep(1.0, 0.97, dist));

    } // end if (dist <= 1.0)
}
