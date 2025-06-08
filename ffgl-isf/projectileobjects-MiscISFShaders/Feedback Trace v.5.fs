/*{
    "CATEGORIES": [
        "Stylize"
    ],
    "CREDIT": "VIDVOX updated by ProjectileObjects ",
    "DESCRIPTION": "",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Feedback Amount",
            "MAX": 1,
            "MIN": -1,
            "NAME": "feedbackAmount",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABELS": [
                "Additive",
                "Mix",
                "Max",
                "Threshold"
            ],
            "NAME": "mixMode",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2,
                3
            ]
        }
    ],
    "ISFVSN": "2",
    "PASSES": [
        {
            "HEIGHT": "$HEIGHT",
            "TARGET": "buffer",
            "WIDTH": "$WIDTH",
            "persistent": true
        }
    ]
}
*/


// Convert RGB to HSV
vec3 rgb2hsv(vec3 c) {
	vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	vec4 p = c.g < c.b ? vec4(c.bg, K.wz) : vec4(c.gb, K.xy);
	vec4 q = c.r < p.x ? vec4(p.xyw, c.r) : vec4(c.r, p.yzx);
	
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

// Convert HSV to RGB
vec3 hsv2rgb(vec3 c) {
	vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void main() {
	vec4 inputPixelColor = IMG_THIS_NORM_PIXEL(inputImage);
	vec4 bufferPixelColor = IMG_THIS_NORM_PIXEL(buffer);
	float logFeedBack = feedbackAmount; // Directly use feedbackAmount
	
	vec4 result = vec4(0.0, 0.0, 0.0, 1.0);
	
	if (mixMode == 0) {
		result.rgb = clamp(inputPixelColor.rgb + bufferPixelColor.rgb * logFeedBack, 0.0, 1.0);
	}
	else if (mixMode == 1) {
		result.rgb = mix(inputPixelColor.rgb, bufferPixelColor.rgb, logFeedBack);
	}
	else if (mixMode == 2) {
		result.rgb = clamp(max(inputPixelColor.rgb, bufferPixelColor.rgb * logFeedBack), 0.0, 1.0);
	}
	else if (mixMode == 3) {
		vec4 feedbackHSV = vec4(1.0);
		feedbackHSV.rgb = rgb2hsv(bufferPixelColor.rgb);
		result.rgb = rgb2hsv(inputPixelColor.rgb);
		if (abs(result.b - sign(feedbackAmount) * feedbackHSV.b) < abs(feedbackAmount)) {
			result.rgb = feedbackHSV.rgb;
		}
		else if (abs(result.r - sign(feedbackAmount) * feedbackHSV.r) < abs(feedbackAmount)) {
			result.rgb = feedbackHSV.rgb;
		}
		result.rgb = hsv2rgb(result.rgb);
	}
	
	result.a = 1.0;
	gl_FragColor = result;
}
