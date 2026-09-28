// The material of a faded model (D-721). An enemy or the Overseer draws with it when its body box crosses the capsule
// of the wall fade, or when the drawn camera is near it. The model keeps the pixels of the 4 by 4 Bayer pattern of the
// wall fade whose threshold is at or under the kept part, so it stays in the opaque pass and needs no sort (D-632).
//
// The model material of every other model is the engine standard material with the atlas, nearest filtering, and
// mipmaps (D-85, D-677). This shader samples the atlas the same way, and it keeps the default light terms.
shader_type spatial;
render_mode cull_back;

uniform sampler2D atlas : source_color, filter_nearest_mipmap;
uniform float kept = 0.3125;

// The threshold of one pixel of the 4 by 4 Bayer pattern, from 1/16 to 16/16.
float dither_threshold(vec2 pixel) {
	int column = int(mod(pixel.x, 4.0));
	int row = int(mod(pixel.y, 4.0));
	int pattern[16] = {0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5};
	return (float(pattern[column + row * 4]) + 1.0) / 16.0;
}

void fragment() {
	if (dither_threshold(FRAGCOORD.xy) > kept) {
		discard;
	}
	ALBEDO = texture(atlas, UV).rgb;
}
