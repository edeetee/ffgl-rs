/*{
    "CREDIT": "Converted by Cornelius // ProjectileObjects",
    "DESCRIPTION": "Labyrinth effect adapted from MadMapper Laser Material",
    "TAGS": "graphic, laser, procedural",
    "VSN": "1.3",
    "INPUTS": [
        { "NAME": "spectrum", "TYPE": "audioFFT", "SIZE": 3 },
        { "LABEL": "Scale", "NAME": "mat_scale", "TYPE": "float", "MIN": 0.15, "MAX": 5.0, "DEFAULT": 1.5 },
        { "LABEL": "Offset", "NAME": "mat_offset", "TYPE": "point2D", "MAX": [1.0, 1.0], "MIN": [-1.0, -1.0], "DEFAULT": [0.0, 0.0] },
        { "LABEL": "Rotation", "NAME": "mat_rotation", "TYPE": "float", "MIN": 0.0, "MAX": 360.0, "DEFAULT": 0.0 },
        { "LABEL": "Auto Move", "NAME": "mat_auto", "TYPE": "bool", "DEFAULT": true },
        { "LABEL": "Auto Move Speed", "NAME": "mat_mspeed", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.4 },
        { "LABEL": "Auto Move Amplitude", "NAME": "mat_amplitude", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
        { "LABEL": "Audio Reactivity", "NAME": "mat_audio_level", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.0 },
        { "LABEL": "Line Thickness", "NAME": "mat_line_thickness", "TYPE": "float", "MIN": 0.005, "MAX": 0.05, "DEFAULT": 0.015 },
        { "LABEL": "Pattern Complexity", "NAME": "mat_pattern_complexity", "TYPE": "float", "MIN": 1.0, "MAX": 10.0, "DEFAULT": 5.0 },
        { "LABEL": "Main Color", "NAME": "mat_mainColor", "TYPE": "color", "DEFAULT": [1.0, 1.0, 1.0, 1.0] },
        { "LABEL": "Background Color", "NAME": "mat_bgColor", "TYPE": "color", "DEFAULT": [0.0, 0.0, 0.0, 1.0] },
        { "LABEL": "Strobe Active", "NAME": "mat_strobeActivated", "TYPE": "bool", "DEFAULT": false },
        { "LABEL": "Strobe Speed", "NAME": "mat_speedStrobe", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6 },
        { "LABEL": "Strobe Duration", "NAME": "mat_strobeDuration", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.25 }
    ]
}*/

mat2 rot(float a) {
    float c = cos(a);
    float s = sin(a);
    return mat2(c, s, -s, c);
}

float labyrinthPattern(vec2 uv, float complexity) {
    uv = abs(fract(uv * complexity) - 0.5);
    return smoothstep(0.1, 0.02, min(uv.x, uv.y));
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    uv = uv * 2.0 - 1.0;
    uv.x *= RENDERSIZE.x / RENDERSIZE.y;
    
    float alteredTime = TIME * mat_mspeed;
    if (mat_auto) {
        uv += vec2(sin(alteredTime) * mat_amplitude, cos(alteredTime) * mat_amplitude);
    }
    
    uv *= rot(radians(mat_rotation));
    uv /= mat_scale * 0.5;
    uv += mat_offset;
    
    float pattern = labyrinthPattern(uv, mat_pattern_complexity);
    vec3 color = mix(mat_bgColor.rgb, mat_mainColor.rgb, pattern);
    
    if (mat_strobeActivated) {
        float strobe = fract(TIME * mat_speedStrobe) < mat_strobeDuration ? 1.0 : 0.0;
        color *= strobe;
    }
    
    gl_FragColor = vec4(color, mat_mainColor.a);
}
