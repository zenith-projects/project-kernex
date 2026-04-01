shader_type canvas_item;

// Animated space-like ambient background with nebula colors and stars.
// Creates visual depth for the glass blur effect to work against.

uniform vec4 color_deep : source_color = vec4(0.02, 0.02, 0.04, 1.0);
uniform vec4 color_nebula1 : source_color = vec4(0.15, 0.06, 0.02, 1.0);
uniform vec4 color_nebula2 : source_color = vec4(0.02, 0.06, 0.12, 1.0);
uniform float speed : hint_range(0.0, 0.5) = 0.08;
uniform float nebula_scale : hint_range(0.5, 5.0) = 2.0;
uniform float star_density : hint_range(0.0, 0.01) = 0.003;

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(
        mix(hash21(i), hash21(i + vec2(1, 0)), f.x),
        mix(hash21(i + vec2(0, 1)), hash21(i + vec2(1, 1)), f.x),
        f.y
    );
}

float fbm(vec2 p) {
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * noise(p);
        p *= 2.1;
        a *= 0.45;
    }
    return v;
}

void fragment() {
    vec2 uv = UV * nebula_scale;
    float t = TIME * speed;

    // Layered nebula
    float n1 = fbm(uv + vec2(t * 0.6, t * 0.2));
    float n2 = fbm(uv * 1.3 + vec2(-t * 0.3, t * 0.5) + 3.7);

    vec3 col = color_deep.rgb;
    col = mix(col, color_nebula1.rgb, n1 * 0.35);
    col = mix(col, color_nebula2.rgb, n2 * 0.25);

    // Tiny stars
    float star = hash21(floor(UV * 400.0));
    if (star > 1.0 - star_density) {
        float twinkle = sin(TIME * (2.0 + star * 4.0)) * 0.3 + 0.7;
        col += vec3(twinkle * (star - (1.0 - star_density)) / star_density);
    }

    // Vignette
    vec2 vc = UV - 0.5;
    col *= 1.0 - dot(vc, vc) * 1.2;

    COLOR = vec4(col, 1.0);
}
