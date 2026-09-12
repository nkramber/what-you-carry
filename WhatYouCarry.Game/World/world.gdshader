// The world material (D-85, D-292). Every chunk shares it. The atlas holds one tile per block, the face
// coordinate repeats across a merged face, and the vertex color carries the occlusion of D-81.
//
// The wall fade: a fragment inside the capsule around the segment from the camera to the player fades. The
// fade is a screen-door dither, so the chunk stays in the opaque pass and needs no sort (OQ-160). The game
// sets the two ends of the segment on every frame.
shader_type spatial;
render_mode cull_back;

uniform sampler2D atlas : source_color, filter_nearest;
uniform vec2 tile_size = vec2(0.125, 0.125);
uniform vec3 fade_start = vec3(0.0, 0.0, 0.0);
uniform vec3 fade_end = vec3(0.0, 0.0, 0.0);
uniform float fade_radius = 0.75;

varying vec3 world_position;

void vertex() {
	world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

// The threshold of one pixel of the 4 by 4 Bayer pattern, from 1/16 to 16/16.
float dither_threshold(vec2 pixel) {
	int column = int(mod(pixel.x, 4.0));
	int row = int(mod(pixel.y, 4.0));
	int pattern[16] = {0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5};
	return (float(pattern[column + row * 4]) + 1.0) / 16.0;
}

void fragment() {
	vec3 segment = fade_end - fade_start;
	float along = clamp(dot(world_position - fade_start, segment) / max(dot(segment, segment), 0.000001), 0.0, 1.0);
	float away = distance(world_position, fade_start + segment * along);
	float fade = 1.0 - clamp(away / fade_radius, 0.0, 1.0);
	if (fade > dither_threshold(FRAGCOORD.xy)) {
		discard;
	}
	vec2 uv = UV2 + fract(UV) * tile_size;
	ALBEDO = texture(atlas, uv).rgb * COLOR.rgb;
}
