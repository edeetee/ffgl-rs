/*{
    "CATEGORIES": [
        "Color",
        "Fx"
    ],
    "CREDIT": "OPTIPHONIC",
    "DESCRIPTION": "Posterise the hue, saturation or value of the input image",
    "INPUTS": [
        {
            "NAME": "hue",
            "TYPE": "float",
            "MIN": 0.0001
        },
        {
            "NAME": "saturation",
            "TYPE": "float",
            "MIN": 0.0001
        },
        {
            "NAME": "value",
            "TYPE": "float",
            "MIN": 0.0001
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

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10f;
    return vec3(abs(q.z + (q.w - q.y) / (6.0f * d + e)), d / (q.x + e), q.x);
}

void main() {
    vec4 in_color = IMG_THIS_PIXEL(inputImage);

    vec3 hsv = rgb2hsv(in_color.rgb);

    hsv.x = round(hsv.x / hue) * hue;
    hsv.y = round(hsv.y / saturation) * saturation;
    hsv.z = round(hsv.z / value) * value;

    vec3 rgb = hsv2rgb(hsv);

    gl_FragColor = vec4(rgb, in_color.a);
}
