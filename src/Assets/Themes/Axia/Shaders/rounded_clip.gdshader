shader_type canvas_item;

uniform float radius : hint_range(0.0, 32.0) = 8.0;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    vec2 size = 1.0 / TEXTURE_PIXEL_SIZE;
    vec2 pixel = UV * size;

    // Distance from each corner
    vec2 tl = vec2(radius);
    vec2 tr = vec2(size.x - radius, radius);
    vec2 bl = vec2(radius, size.y - radius);
    vec2 br = size - vec2(radius);

    float alpha = 1.0;

    if (pixel.x < radius && pixel.y < radius)
        alpha = 1.0 - smoothstep(radius - 1.0, radius, distance(pixel, tl));
    else if (pixel.x > size.x - radius && pixel.y < radius)
        alpha = 1.0 - smoothstep(radius - 1.0, radius, distance(pixel, tr));
    else if (pixel.x < radius && pixel.y > size.y - radius)
        alpha = 1.0 - smoothstep(radius - 1.0, radius, distance(pixel, bl));
    else if (pixel.x > size.x - radius && pixel.y > size.y - radius)
        alpha = 1.0 - smoothstep(radius - 1.0, radius, distance(pixel, br));

    COLOR = vec4(tex.rgb, tex.a * alpha);
}
