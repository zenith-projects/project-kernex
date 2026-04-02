shader_type canvas_item;

// Background enhancement: zoom breathing, barrel distortion, vignette, color grading.

uniform float breath_amount : hint_range(0.0, 0.1) = 0.035;
uniform float breath_speed : hint_range(0.0, 1.0) = 0.3;
uniform float barrel_base : hint_range(0.0, 0.3) = 0.05;
uniform float barrel_breath : hint_range(0.0, 0.3) = 0.08;
uniform float vignette_strength : hint_range(0.0, 1.0) = 0.4;
uniform float vignette_softness : hint_range(0.1, 2.0) = 0.7;
uniform float brightness : hint_range(0.5, 1.5) = 0.92;
uniform float contrast : hint_range(0.5, 1.5) = 1.05;

void fragment() {
    float breath_phase = sin(TIME * breath_speed);

    // Zoom breathing
    float breath = 1.0 - breath_amount + breath_phase * breath_amount;

    // Barrel distortion — stronger during breath peak
    float barrel = barrel_base + breath_phase * barrel_breath * 0.5;
    vec2 centered = UV - 0.5;
    float dist2 = dot(centered, centered);
    vec2 distorted = centered * (1.0 + barrel * dist2);

    // Apply breathing scale on top of distortion, clamp to bounds
    vec2 uv = clamp(distorted * breath + 0.5, 0.001, 0.999);

    vec4 tex = texture(TEXTURE, uv);

    // Contrast + brightness
    vec3 col = tex.rgb;
    col = (col - 0.5) * contrast + 0.5;
    col *= brightness;

    // Vignette
    float vignette = 1.0 - smoothstep(vignette_softness, vignette_softness + 0.4, length(centered) * 1.8);
    col *= mix(1.0 - vignette_strength, 1.0, vignette);

    COLOR = vec4(col, tex.a);
}
