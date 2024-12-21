/*{
    "CATEGORIES": [
        "Audio Reactive"
    ],
    "CREDIT": "https://www.shadertoy.com/user/yozic",
    "DESCRIPTION": "From: https://www.shadertoy.com/view/WtGfRw",
    "INPUTS": [
        {
            "DEFAULT": 0.07,
            "NAME": "zoom",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.13,
            "NAME": "contrast",
            "TYPE": "float"
        },
        {
            "DEFAULT": 11,
            "MAX": 100,
            "NAME": "radius",
            "TYPE": "float"
        },
        {
            "DEFAULT": 10.32,
            "MAX": 100,
            "NAME": "colorShift",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "NAME": "rotation",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "MAX": 10,
            "MIN": -10,
            "NAME": "sinMul",
            "TYPE": "float"
        },
        {
            "DEFAULT": 2.38,
            "MAX": 10,
            "MIN": -10,
            "NAME": "cosMul",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "NAME": "yMul",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.28,
            "NAME": "xMul",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "NAME": "xSpeed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "NAME": "ySpeed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.003,
            "NAME": "gloop",
            "TYPE": "float"
        },
        {
            "DEFAULT": 4.99,
            "NAME": "yDivide",
            "TYPE": "float"
        },
        {
            "DEFAULT": 6.27,
            "NAME": "xDivide",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2"
}
*/

vec3 iResolution = vec3(RENDERSIZE, 1.f);
float iTime = TIME;

#define PI 3.141592
#define orbs 20.

/*

Variant 01 

#define zoom 0.07
#define contrast 0.13
#define colorShift 10.32
#define radius 11.
#define rotation 1.
#define sinMul 0.
#define cosMul 2.38
#define yMul 0.
#define xMul 0.28
#define xSpeed 0.
#define ySpeed 0.
#define gloop 0.003;
#define yDivide 4.99
#define xDivide 6.27

*/

#define orbSize 6.46
#define sides 1.

/*

Variant 02

#define zoom 0.27
#define contrast 0.13
#define orbSize 4.25
#define radius 11.
#define colorShift 10.32
#define sides 1.
#define rotation 1.
#define sinMul 0.
#define cosMul 2.38
#define yMul 0.
#define xMul 0.28
#define xSpeed 0.
#define ySpeed 0.
#define gloop 0.003
#define yDivide 11.
#define xDivide 12.4

*/

/*

Variant 03

#define zoom 0.02
#define contrast 0.13
#define orbSize 11.
#define radius 3.21
#define colorShift 10.32
#define sides 1.
#define rotation 1.
#define sinMul 0.
#define cosMul 5.
#define yMul 0.
#define xMul 0.28
#define xSpeed 0.
#define ySpeed 0.
#define gloop 0.003
#define yDivide 10.99
#define xDivide 12.

*/

vec4 orb(vec2 uv, float s, vec2 p, vec3 color, float c) {
    return pow(vec4(s / length(uv + p) * color, 1.f), vec4(c));
}

mat2 rotate(float a) {
    return mat2(cos(a), -sin(a), sin(a), cos(a));
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (2.f * fragCoord - iResolution.xy) / iResolution.y;
    uv *= zoom;
    uv /= dot(uv, uv);
    uv *= rotate(rotation * iTime / 10.f);
    vec4 c = vec4(0.f);
    for(float i = 0.f; i < orbs; i++) {
        uv.x += sinMul * sin(uv.y * yMul + iTime * xSpeed) + cos(uv.y / yDivide - iTime);
        uv.y += cosMul * cos(uv.x * xMul - iTime * ySpeed) - sin(uv.x / xDivide - iTime);
        float t = i * PI / orbs * 2.f;
        float x = radius * tan(t);
        float y = radius * cos(t + iTime / 10.f);
        vec2 position = vec2(x, y);
        vec3 color = cos(.02f * uv.x + .02f * uv.y * vec3(-2, 0, -1) * PI * 2.f / 3.f + PI * (float(i) / colorShift)) * 0.5f + 0.5f;
        c += .65f - orb(uv, orbSize, position, 1.f - color, contrast);
    }
    c.a = 1.0f;
    fragColor = c;
}

void main(void) {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}