shader_type canvas_item;

// Frosted glass: blurs screen behind, adds tint + subtle grain.
// Apply to a ColorRect covering the panel area.
// Works best when there's varied content behind (game world, gradients).

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap;
uniform float blur_amount : hint_range(0.0, 6.0) = 3.0;
uniform vec4 tint_color : source_color = vec4(0.06, 0.06, 0.08, 0.82);
uniform float noise_amount : hint_range(0.0, 0.06) = 0.02;
uniform float saturation : hint_range(0.0, 1.0) = 0.4;

float hash(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

void fragment() {
    // LOD-based blur (GPU efficient, single sample)
    vec4 blurred = textureLod(screen_texture, SCREEN_UV, blur_amount);

    // Desaturate blurred background slightly
    float luma = dot(blurred.rgb, vec3(0.299, 0.587, 0.114));
    blurred.rgb = mix(vec3(luma), blurred.rgb, saturation);

    // Tint overlay
    vec3 result = mix(blurred.rgb, tint_color.rgb, tint_color.a);

    // Subtle grain for texture
    float grain = (hash(SCREEN_UV * 800.0 + TIME * 0.3) - 0.5) * noise_amount;
    result += vec3(grain);

    COLOR = vec4(result, 1.0);
}
