/*{
	"DESCRIPTION": "VHS Glitch Style",
	"CREDIT": "David Lublin, original by Staffan Widegarn Åhlvik",
	"ISFVSN": "2",
	"CATEGORIES": [
		"Stylize", "Glitch", "Retro"
	],
	"INPUTS": [
		{
			"NAME": "inputImage",
			"TYPE": "image"
		},
		{
			"NAME": "autoScan",
			"TYPE": "bool",
			"DEFAULT": 1.0
		},
		{
			"NAME": "xScanline",
			"TYPE": "float",
			"DEFAULT": 0.5,
			"MIN": 0.0,
			"MAX": 1.0
		},
		{
			"NAME": "xScanline2",
			"TYPE": "float",
			"DEFAULT": 0.5,
			"MIN": 0.0,
			"MAX": 1.0
		},
		{
			"NAME": "yScanline",
			"TYPE": "float",
			"DEFAULT": 0.0,
			"MIN": 0.0,
			"MAX": 1.0
		},
		{
			"NAME": "xScanlineSize",
			"TYPE": "float",
			"DEFAULT": 0.5,
			"MIN": 0.0,
			"MAX": 1.0
		},
		{
			"NAME": "xScanlineSize2",
			"TYPE": "float",
			"DEFAULT": 0.25,
			"MIN": 0.0,
			"MAX": 1.0
		},
		{
			"NAME": "yScanlineAmount",
			"TYPE": "float",
			"DEFAULT": 0.25,
			"MIN": -1.0,
			"MAX": 1.0
		},
		{
			"NAME": "grainLevel",
			"TYPE": "float",
			"DEFAULT": 0.0,
			"MIN": 0.0,
			"MAX": 3.0
		},
		{
			"NAME": "scanFollow",
			"TYPE": "bool",
			"DEFAULT": 1.0
		},
		{
			"NAME": "analogDistort",
			"TYPE": "float",
			"DEFAULT": 1.0,
			"MIN": 0.0,
			"MAX": 10.0
		},
		{
			"NAME": "bleedAmount",
			"TYPE": "float",
			"DEFAULT": 1.0,
			"MIN": 0.0,
			"MAX": 3.0
		},
		{
			"NAME": "bleedDistort",
			"TYPE": "float",
			"DEFAULT": 0.5,
			"MIN": 0.0,
			"MAX": 1.0
		},
		{
			"NAME": "bleedRange",
			"TYPE": "float",
			"DEFAULT": 1.0,
			"MIN": 0.0,
			"MAX": 2.0
		},
		{
			"NAME": "colorBleedL",
			"TYPE": "color",
			"DEFAULT": [
				0.8,
				0.0,
				0.4,
				1.0
			]
		},
		{
			"NAME": "colorBleedC",
			"TYPE": "color",
			"DEFAULT": [
				0.0,
				0.5,
				0.75,
				1.0
			]
		},
		{
			"NAME": "colorBleedR",
			"TYPE": "color",
			"DEFAULT": [
				0.8,
				0.0,
				0.4,
				1.0
			]
		}
	]
	
}*/

//	Based on https://github.com/staffantan/unity-vhsglitch
//	Converted by David Lublin / VIDVOX

const float tau = 6.28318530718f;

float rand(vec3 co) {
    return abs(mod(sin(dot(co.xyz, vec3(12.9898f, 78.233f, 45.5432f))) * 43758.5453f, 1.0f));
}

void main() {
    float actualXLine = (!autoScan) ? xScanline : mod(xScanline + ((1.0f + sin(0.34f * TIME)) / 2.0f + (1.0f + sin(TIME)) / 3.0f + (1.0f + cos(2.1f * TIME)) / 3.0f + (1.0f + cos(0.027f * TIME)) / 2.0f) / 3.5f, 1.0f);
    float actualXLineWidth = (!autoScan) ? xScanlineSize : xScanlineSize + ((1.0f + sin(1.2f * TIME)) / 2.0f + (1.0f + cos(3.91f * TIME)) / 3.0f + (1.0f + cos(0.014f * TIME)) / 2.0f) / 3.5f;
    vec2 loc = isf_FragNormCoord;
    vec4 vhs = IMG_NORM_PIXEL(inputImage, loc);
    float dx = 1.0f + actualXLineWidth / 25.0f - abs(distance(loc.y, actualXLine));
    float dx2 = 1.0f + xScanlineSize2 / 10.0f - abs(distance(loc.y, xScanline2));
    float dy = (1.0f - abs(distance(loc.y, yScanline)));
    if(autoScan)
        dy = (1.0f - abs(distance(loc.y, mod(yScanline + TIME, 1.0f))));

    dy = (dy > 0.5f) ? 2.0f * dy : 2.0f * (1.0f - dy);

    float rX = (scanFollow) ? rand(vec3(dy, actualXLine, analogDistort)) : rand(vec3(dy, bleedAmount, analogDistort));
    float xTime = (actualXLine > 0.5f) ? 2.0f * actualXLine : 2.0f * (1.0f - actualXLine);

    loc.x += yScanlineAmount * dy * 0.025f + analogDistort * rX / (RENDERSIZE.x / 2.0f);

    if(dx2 > 1.0f - xScanlineSize2 / 10.0f) {
        float rX2 = (dy * rand(vec3(dy, dx2, dx + TIME)) + dx2) / 4.0f;
        float distortAmount = analogDistort * (sin(rX * tau / dx2) + cos(rX * tau * 0.78f / dx2)) / 10.0f;
		//loc.y = xScanline2;
		//loc.x += (1.0 + distortAmount * sin(tau * (loc.x) / rX2 ) - 1.0) / 15.0;
        loc.x += (1.0f + distortAmount * sin(tau * (loc.x) / rX2) - 1.0f) / 15.0f;
    }
    if(dx > 1.0f - actualXLineWidth / 25.0f)
        loc.y = actualXLine;

    loc.x = mod(loc.x, 1.0f);
    loc.y = mod(loc.y, 1.0f);

    vec4 c = IMG_NORM_PIXEL(inputImage, loc);
    float x = (loc.x * 320.0f) / 320.0f;
    float y = (loc.y * 240.0f) / 240.0f;
    float bleed = 0.0f;

    if(scanFollow)
        c -= rand(vec3(x, y, xTime)) * xTime / (5.0f - grainLevel);
    else
        c -= rand(vec3(x, y, bleedAmount)) * (bleedAmount / 20.0f) / (5.0f - grainLevel);

    if(bleedAmount > 0.0f) {
        IMG_NORM_PIXEL(inputImage, loc + vec2(0.01f, 0)).r;
        bleed += IMG_NORM_PIXEL(inputImage, loc + bleedRange * vec2(0.02f, 0)).r;
        bleed += IMG_NORM_PIXEL(inputImage, loc + bleedRange * vec2(0.01f, 0.01f)).r;
        bleed += IMG_NORM_PIXEL(inputImage, loc + bleedRange * vec2(-0.02f, 0.02f)).r;
        bleed += IMG_NORM_PIXEL(inputImage, loc + bleedRange * vec2(0.0f, -0.03f)).r;
        bleed /= 6.0f;
        bleed *= bleedAmount;
    }

    if(bleed > 0.1f) {
        float bleedFreq = 1.0f;
        float bleedX = 0.0f;
        if(autoScan)
            bleedX = x + bleedDistort * (yScanlineAmount + (1.5f + cos(TIME / 13.0f + tau * (bleedDistort + (1.0f - loc.y)))) / 2.0f) * sin((TIME / 9.0f + bleedDistort) * tau + loc.y * loc.y * tau * bleedFreq) / 8.0f;
        else
            bleedX = x + (yScanlineAmount + (1.0f + sin(tau * (bleedDistort + loc.y))) / 2.0f) * sin(bleedDistort * tau + loc.y * loc.y * tau * bleedFreq) / 10.0f;
        vec4 colorBleed = (bleedX < 0.5f) ? mix(colorBleedL, colorBleedC, 2.0f * bleedX) : mix(colorBleedR, colorBleedC, 2.0f - 2.0f * bleedX);
        if(scanFollow)
            c += bleed * max(xScanlineSize, xTime) * colorBleed;
        else
            c += bleed * colorBleed;
    }
    gl_FragColor = c;
}
