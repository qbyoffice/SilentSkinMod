shader_type canvas_item;
//makes noise texture
uniform sampler2D noise : repeat_enable;
uniform sampler2D mask;
uniform float speed : hint_range(0.002,0.5) = 0.1;
uniform float strength : hint_range(0.002,0.5) = 0.1;

void fragment() {
	//sample noise texture
	float noise_value = texture(noise,UV+-TIME*speed).r;
	vec4 col = texture(TEXTURE, UV);
	vec4 distorted = texture(TEXTURE,UV - (strength * 0.5) + vec2(noise_value) * (strength * 0.5));

	// Distort if mask texture alpha is above 0.1
	float distort = step(texture(mask, UV).a, 0.1);
	COLOR = col * distort + distorted * (1.0 - distort);
	//COLOR = vec4(distort, distort, distort, 1.0);
}
