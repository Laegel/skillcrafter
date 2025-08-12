shader_type canvas_item;

uniform sampler2D frost_noise : hint_default_white;
uniform float gloss_strength = 0.3;
uniform float time_speed = 0.5;

void fragment() {
    vec2 uv = UV;

    // Frost shimmer
    vec2 frost_uv = UV * 6.0 + vec2(TIME * time_speed, 0.0);
    float frost = texture(frost_noise, frost_uv).r;

    // Base texture
    vec4 base = texture(TEXTURE, uv);

    // Add shimmer to highlights
    vec3 gloss_color = vec3(0.8, 0.9, 1.0);
    base.rgb += gloss_color * frost * gloss_strength;

    COLOR = base;
}
