/*{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "CREDIT": "Modified by ProjectileObjects",
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/llVBDD by starea.  A naïve but awesome-looking transfer function for visualizing the depth map.\nUsed in Social Street View: Blending Immersive Street Views with Geo-tagged Social Media. Ruofei Du and Amitabh Varshney.",
    "IMPORTED": {
        "iChannel0": {
            "NAME": "iChannel0",
            "PATH": "8de3a3924cb95bd0e95a443fff0326c869f9d4979cd1d5b6e94e2a01f5be53e9.jpg"
        }
    },
    "INPUTS": [
        {
            "LABEL": "camera",
            "NAME": "camera",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0.21,
            "LABEL": "color1",
            "NAME": "val1",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.71,
            "LABEL": "color2",
            "NAME": "val2",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.07,
            "LABEL": "color3",
            "NAME": "val3",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "Threshold",
            "NAME": "val4",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2"
}
*/


// Free to use. 
// ruofeidu.com

float grayScale(in vec3 col)
{
    return dot(col, vec3(val1, val2, val3));
}

vec3 transfer(in float x)
{
    float s = step(val4, x);
    float y = 1.0 - 2.0 * x;
    return vec3(s * max(-0.98, y) + 1.0, 
                (1.0 - s) * y, 
                0.0);
}

void main() {



    vec2 uv = gl_FragCoord.xy/RENDERSIZE.xy;
    float x = grayScale(IMG_NORM_PIXEL(camera,mod(uv,1.0)).rgb);
    gl_FragColor = vec4(transfer(x), 1.0);
    
}
