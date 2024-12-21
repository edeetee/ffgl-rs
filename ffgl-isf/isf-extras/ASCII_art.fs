/*{
	"DESCRIPTION": "ASCII Art",
	"CREDIT": "by VIDVOX (Ported from https://www.shadertoy.com/view/lssGDj)",
	"ISFVSN": "2",
	"CATEGORIES": [
		"Stylize", "Retro"
	],
	"INPUTS": [
		{
			"NAME": "inputImage",
			"TYPE": "image"
		},
		{
			"NAME": "size",
			"TYPE": "float",
			"MIN": 0.0,
			"MAX": 1.0,
			"DEFAULT": 0.1
		},
		{
			"NAME": "gamma",
			"TYPE": "float",
			"DEFAULT": 1.0,
			"MIN": 0.5,
			"MAX": 2.0
		},
		{
			"NAME": "tint",
			"TYPE": "float",
			"MIN": 0.0,
			"MAX": 1.0,
			"DEFAULT": 1.0
		},
		{
			"NAME": "tintColor",
			"TYPE": "color",
			"DEFAULT": [
				0.0,
				1.0,
				0.0,
				1.0
			]
		},
		{
			"NAME": "alphaMode",
			"TYPE": "bool",
			"DEFAULT": 0.0
		}
	]
	
}*/

float character(float n, vec2 p) // some compilers have the word "char" reserved
{
    p = floor(p * vec2(4.0f, -4.0f) + 2.5f);
    if(clamp(p.x, 0.0f, 4.0f) == p.x && clamp(p.y, 0.0f, 4.0f) == p.y) {
        if(int(mod(n / exp2(p.x + 5.0f * p.y), 2.0f)) == 1)
            return 1.0f;
    }
    return 0.0f;
}

void main() {
    float _size = size * 36.0f + 8.0f;
    vec2 uv = gl_FragCoord.xy;
    vec4 inputColor = IMG_NORM_PIXEL(inputImage, (floor(uv / _size) * _size / RENDERSIZE.xy));
    vec3 col = inputColor.rgb;
    float gray = (col.r + col.g + col.b) / 3.0f;
    gray = pow(gray, gamma);
    col = mix(tintColor.rgb, col.rgb, 1.0f - tint);

    float n = 65536.0f;             // .
    if(gray > 0.2f)
        n = 65600.0f;    // :
    if(gray > 0.3f)
        n = 332772.0f;   // *
    if(gray > 0.4f)
        n = 15255086.0f; // o 
    if(gray > 0.5f)
        n = 23385164.0f; // &
    if(gray > 0.6f)
        n = 15252014.0f; // 8
    if(gray > 0.7f)
        n = 13199452.0f; // @
    if(gray > 0.8f)
        n = 11512810.0f; // #

    vec2 p = mod(uv / (_size / 2.0f), 2.0f) - vec2(1.0f);
    col = col * character(n, p);
    float alpha = mix(tintColor.a * inputColor.a, inputColor.a, 1.0f - tint);
    if(alphaMode) {
        alpha = (col.r + col.g + col.b) / 3.0f;
        alpha = (alpha > 0.01f) ? tintColor.a : alpha;
    }

    gl_FragColor = vec4(col, alpha);

}
