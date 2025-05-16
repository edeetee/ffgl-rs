// Created by Stephane Cuillerdier - @Aiekick/2016
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// Tuned via XShade (http://www.funparadigm.com/xshade/)

/* 
	Based on shane shader : https://www.shadertoy.com/view/ll2SRy
*/

mat3 getRotZMat(float a) {
    return mat3(cos(a), -sin(a), 0.f, sin(a), cos(a), 0.f, 0.f, 0.f, 1.f);
}

float dstepf = 0.0f;

float map(vec3 p) {
    p.x += sin(p.z * 1.8f);
    p.y += cos(p.z * .2f) * sin(p.x * .8f);
    p *= getRotZMat(p.z * 0.8f + sin(p.x) + cos(p.y));
    p.xy = mod(p.xy, 0.3f) - 0.15f;
    dstepf += 0.003f;
    return length(p.xy);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - iResolution.xy * .5f) / iResolution.y;
    vec3 rd = normalize(vec3(uv, (1.f - dot(uv, uv) * .5f) * .5f));
    vec3 ro = vec3(0, 0, iTime * 1.26f), col = vec3(0), sp;
    float cs = cos(iTime * 0.375f), si = sin(iTime * 0.375f);
    rd.xz = mat2(cs, si, -si, cs) * rd.xz;
    float t = 0.06f, layers = 0.f, d = 0.f, aD;
    float thD = 0.02f;
    for(float i = 0.f; i < 250.f; i++) {
        if(layers > 15.f || col.x > 1.f || t > 5.6f)
            break;
        sp = ro + rd * t;
        d = map(sp);
        aD = (thD - abs(d) * 15.f / 16.f) / thD;
        if(aD > 0.f) {
            col += aD * aD * (3.f - 2.f * aD) / (1.f + t * t * 0.25f) * .2f;
            layers++;
        }
        t += max(d * .7f, thD * 1.5f) * dstepf;
    }
    col = max(col, 0.f);
    col = mix(col, vec3(min(col.x * 1.5f, 1.f), pow(col.x, 2.5f), pow(col.x, 12.f)), dot(sin(rd.yzx * 8.f + sin(rd.zxy * 8.f)), vec3(.1666f)) + 0.4f);
    col = mix(col, vec3(col.x * col.x * .85f, col.x, col.x * col.x * 0.3f), dot(sin(rd.yzx * 4.f + sin(rd.zxy * 4.f)), vec3(.1666f)) + 0.25f);
    fragColor = vec4(clamp(col, 0.f, 1.f), 1.0f);
}
