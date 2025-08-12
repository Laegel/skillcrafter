shader_type canvas_item;

uniform sampler2D noise_texture : hint_default_white;
uniform vec4 water_color : source_color = vec4(0.2, 0.5, 0.7, 1.0); // Default: bluish
uniform float distortion_strength = 0.005;
uniform float fresnel_strength = 1.5;
uniform float fresnel_power = 2.0;

void fragment() {
    vec2 uv = UV;

    // Sample noise to create subtle surface distortion
    vec2 noise_uv = UV * vec2(8.0, 8.0); // Adjust scale
    vec2 noise = texture(noise_texture, noise_uv).rg - 0.5;
    uv += noise * distortion_strength;

    // Base color with distortion
    vec4 base_color = texture(TEXTURE, uv);

    // Optional: Fresnel-like edge highlight based on distance to center
    vec2 center = vec2(0.5, 0.5);
    float dist = distance(UV, center);
    float fresnel = pow(1.0 - dist, fresnel_power) * fresnel_strength;

    // Blend highlight slightly (like light grazing the surface)
    vec3 highlight = vec3(0.9, 0.9, 1.0);
    base_color.rgb += water_color.rgb - highlight * fresnel;

	base_color.a *= water_color.a;
    COLOR = base_color;
}
