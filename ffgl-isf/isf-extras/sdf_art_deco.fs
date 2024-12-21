/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/mds3DX by morisil.  I just played a bit with the coefficients of my original \"generative art deco\". I changed color grading to be based on polar coordinates and put the shape in motion.",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "NAME": "iChannel0",
            "TYPE": "audio"
        }
    ]
}

*/

// Fork of "generative art deco 3" by morisil. https://shadertoy.com/view/mdl3WX
// 2022-10-28 00:47:55

// Fork of "generative art deco 2" by morisil. https://shadertoy.com/view/ftVBDz
// 2022-10-27 22:34:54

// Fork of "generative art deco" by morisil. https://shadertoy.com/view/7sKfDd
// 2022-09-28 11:25:15

// Copyright Kazimierz Pogoda, 2022 - https://xemantic.com/
// I am the sole copyright owner of this Work.
// You cannot host, display, distribute or share this Work in any form,
// including physical and digital. You cannot use this Work in any
// commercial or non-commercial product, website or project. You cannot
// sell this Work and you cannot mint an NFTs of it.
// I share this Work for educational purposes, and you can link to it,
// through an URL, proper attribution and unmodified screenshot, as part
// of your educational material. If these conditions are too restrictive
// please contact me and we'll definitely work it out.

// copyright statement borrowed from Inigo Quilez

// Music by Giovanni Sollima, L'invenzione del nero:
// https://soundcloud.com/giovanni-sollima/linvenzione-del-nero

// See also The Mathematics of Perception to check the ideas behind:
// https://www.shadertoy.com/view/7sVBzK

const float SHAPE_SIZE = .618f;
const float CHROMATIC_ABBERATION = .01f;
const float ITERATIONS = 10.f;
const float INITIAL_LUMA = .5f;

const float PI = 3.14159265359f;
const float TWO_PI = 6.28318530718f;

mat2 rotate2d(float _angle) {
    return mat2(cos(_angle), -sin(_angle), sin(_angle), cos(_angle));
}

float sdPolygon(in float angle, in float distance) {
    float segment = TWO_PI / 4.0f;
    return cos(floor(.5f + angle / segment) * segment - angle) * distance;
}

float getColorComponent(in vec2 st, in float modScale, in float blur) {
    vec2 modSt = mod(st, 1.f / modScale) * modScale * 2.f - 1.f;
    float dist = length(modSt);
    float angle = atan(modSt.x, modSt.y) + sin(TIME * .08f) * 9.0f;
    //dist = sdPolygon(angle, dist);
    //dist += sin(angle * 3. + TIME * .21) * .2 + cos(angle * 4. - TIME * .3) * .1;
    float shapeMap = smoothstep(SHAPE_SIZE + blur, SHAPE_SIZE - blur, sin(dist * 3.0f) * .5f + .5f);
    return shapeMap;
}

void main() {

    float blur = .4f + sin(TIME * .52f) * .2f;
    vec2 st = (2.f * gl_FragCoord.xy - RENDERSIZE.xy) / min(RENDERSIZE.x, RENDERSIZE.y);
    vec2 origSt = st;
    st *= rotate2d(sin(TIME * .14f) * .3f);
    st *= (sin(TIME * .15f) + 2.f) * .3f;
    st *= log(length(st * .428f)) * 1.1f;
    float modScale = 1.f;
    vec3 color = vec3(0);
    float luma = INITIAL_LUMA;
    for(float i = 0.f; i < ITERATIONS; i++) {
        vec2 center = st + vec2(sin(TIME * .12f), cos(TIME * .13f));
        float fft = IMG_NORM_PIXEL(iChannel0, mod(vec2(length(center), .25f), 1.0f)).r;

        //center += pow(length(center), 1.);
        vec3 shapeColor = vec3(getColorComponent(center - st * CHROMATIC_ABBERATION, modScale, blur), getColorComponent(center, modScale, blur), getColorComponent(center + st * CHROMATIC_ABBERATION, modScale, blur)) * luma;
        st *= 1.1f + getColorComponent(center, modScale, .04f) * 1.2f;
        st *= rotate2d(sin(TIME * .05f) * 1.33f);
        color += shapeColor;
        color = clamp(color, 0.f, 1.f);
//        if (color == vec3(1)) break;
        luma *= .6f;
        blur *= .63f;
    }
    const float GRADING_INTENSITY = .4f;
    vec3 topGrading = vec3(1.f + sin(TIME * 1.13f * .3f) * GRADING_INTENSITY, 1.f + sin(TIME * 1.23f * .3f) * GRADING_INTENSITY, 1.f - sin(TIME * 1.33f * .3f) * GRADING_INTENSITY);
    vec3 bottomGrading = vec3(1.f - sin(TIME * 1.43f * .3f) * GRADING_INTENSITY, 1.f - sin(TIME * 1.53f * .3f) * GRADING_INTENSITY, 1.f + sin(TIME * 1.63f * .3f) * GRADING_INTENSITY);
    float origDist = length(origSt);
    vec3 colorGrading = mix(topGrading, bottomGrading, origDist - .5f);
    gl_FragColor = vec4(pow(color.rgb, colorGrading), 1.f);
    gl_FragColor *= smoothstep(2.1f, .7f, origDist);
}
