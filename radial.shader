shader_type canvas_item;

uniform vec4 inner_color : source_color = vec4(1, 1, 1, 1);
uniform vec4 outer_color : source_color = vec4(0, 0, 0, 1);
uniform float radius: hint_range(0, 1) = 1;

void fragment() {
    // Calculate the distance from the center
    float dist = distance(UV, vec2(0.5, 0.5));

    // Smoothly interpolate between inner_color and outer_color over a wider range
    vec4 gradient_color = mix(inner_color, outer_color, smoothstep(0.0, radius * 2.0, dist));

    // Set the final color
    COLOR = gradient_color;

    // Optional: Make the gradient fade out completely outside the radius
    if (dist > radius) {
        COLOR.a = 0.0;
    }
}
