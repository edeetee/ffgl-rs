/*{
    "CATEGORIES": [
        "Color",
        "Fx"
    ],
    "CREDIT": "OPTIPHONIC",
    "DESCRIPTION": "Adjust the hue, saturation, and value of the input image",
    "INPUTS": [
        {
            "NAME": "mult",
            "TYPE": "color",
            "DEFAULT": [1.0, 0.0, 1.0, 0.0]
        },
        {
            "NAME": "invert",
            "TYPE": "color",
            "DEFAULT": [0.0,0.0,0.0,0.0]
        },
        {
            "NAME": "powVal",
            "TYPE": "color",
            "DEFAULT": [0.5, 0.5, 0.5, 0.0]
        },
        {
            "NAME": "add",
            "TYPE": "color",
            "DEFAULT": [0.0,1.0,0.0,0.0],
            "MIN": [-1.0, -1.0, -1.0, -1.0],
            "MAX": [1.0, 1.0, 1.0, 1.0]
        },
        {
            "NAME": "monochrome",
            "TYPE": "bool",
            "DEFAULT": true
        },
        {
            "NAME": "inputImage",
            "TYPE": "image"
        }
    ],
    "ISFVSN": "2"
}
*/

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0f - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0f, 1.0f), c.y);
}

void main() {
    vec4 in_color = IMG_THIS_PIXEL(inputImage);

    vec4 val = in_color;

    if(monochrome) {
        val = vec4((val.r + val.g + val.b) / 3.0f);
    }

    vec4 hsv = mult * val;

    vec4 invert_mult = (vec4(0.5f) - invert) * 2;
    hsv = invert + hsv * invert_mult;

    hsv = pow(hsv, pow(vec4(2.0f), (powVal - 0.5f) * 4));

    hsv += add;

    hsv.g = clamp(hsv.g, 0.0f, 1.0f);
    hsv.b = clamp(hsv.b, 0.0f, 1.0f);

	// fragColor = TDOutputSwizzle(vec4(hsv, in_color.a));
	// return;

    vec3 rgb = hsv2rgb(hsv.xyz);

	// vec4 in_color = vec4(1.0);
    fragColor = vec4(rgb, in_color.a);
}
