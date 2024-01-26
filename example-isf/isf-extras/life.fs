/*{
  "DESCRIPTION": "Based on Conway Game of Life",
  "CREDIT": "VIDVOX",
  "CATEGORIES": [
    "Generator"
  ],
  "INPUTS": [
    {
      "NAME": "restartNow",
      "TYPE": "event"
    },
    {
      "NAME": "startThresh",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "randomRegrowth",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 0.1
    },
    {
      "NAME": "randomDeath",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 0.1
    }
  ],
  "PASSES": [
    {
      "TARGET": "lastData",
      "persistent": true
    }
  ]
}*/

/*

Any live cell with fewer than two live neighbours dies, as if caused by under-population.
Any live cell with two or three live neighbours lives on to the next generation.
Any live cell with more than three live neighbours dies, as if by over-population.
Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.

*/

float gray(vec4 n) {
	return (n.r + n.g + n.b) / 3.0;
}

float hash12(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * .1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
}

void main() {
	vec4 inputPixelColor = vec4(0.0);

	vec2 texc = isf_FragNormCoord.xy * RENDERSIZE;

	vec2 loc = texc;

	vec2 left_coord = vec2(texc.xy + vec2(-1.0, 0));
	vec2 right_coord = vec2(texc.xy + vec2(1.0, 0));
	vec2 above_coord = vec2(texc.xy + vec2(0, 1.0));
	vec2 below_coord = vec2(texc.xy + vec2(0, -1.0));

	vec2 lefta_coord = vec2(texc.xy + vec2(-1.0, 1.0));
	vec2 righta_coord = vec2(texc.xy + vec2(1.0, 1.0));
	vec2 leftb_coord = vec2(texc.xy + vec2(-1.0, -1.0));
	vec2 rightb_coord = vec2(texc.xy + vec2(1.0, -1.0));

	if((TIME < 0.5) || (restartNow)) {
		//	randomize the start conditions
		float alive = hash12(vec2(TIME + 1.0, 2.1 * TIME + 0.1) + loc + vec2(1232));
		if(alive > 1.0 - startThresh) {
			inputPixelColor = vec4(1.0);
		}
	} else {
		vec4 color = IMG_PIXEL(lastData, loc);
		vec4 colorL = IMG_PIXEL(lastData, left_coord);
		vec4 colorR = IMG_PIXEL(lastData, right_coord);
		vec4 colorA = IMG_PIXEL(lastData, above_coord);
		vec4 colorB = IMG_PIXEL(lastData, below_coord);

		vec4 colorLA = IMG_PIXEL(lastData, lefta_coord);
		vec4 colorRA = IMG_PIXEL(lastData, righta_coord);
		vec4 colorLB = IMG_PIXEL(lastData, leftb_coord);
		vec4 colorRB = IMG_PIXEL(lastData, rightb_coord);

		float neighborSum = gray(colorL + colorR + colorA + colorB + colorLA + colorRA + colorLB + colorRB);
		float state = gray(color);

		//	live cell
		if(state > 0.0) {
			if(neighborSum < 2.0) {
				//	under population
				inputPixelColor = vec4(0.0);
			} else if(neighborSum < 4.0) {
				//	status quo
				inputPixelColor = vec4(1.0);

				//	spontaneous death?
				float alive = hash12(vec2(TIME + 1.0, 2.1 * TIME + 0.1) + loc + vec2(0312230.5123));
				if(alive > 1.0 - randomDeath) {
					inputPixelColor = vec4(0.0);
				}
			} else {
				//	over population
				inputPixelColor = vec4(0.0);
			}
		}
		//	dead cell
		else {
			if((neighborSum > 2.0) && (neighborSum < 4.0)) {
				//	reproduction
				inputPixelColor = vec4(1.0);
			} else if(neighborSum < 2.0) {
				//	spontaneous reproduction
				float alive = hash12(vec2(TIME + 1.0, 2.1 * TIME + 0.1) + loc + vec2(0.02312));
				if(alive > 1.0 - randomRegrowth) {
					inputPixelColor = vec4(1.0);
				}
			}
		}
	}

	gl_FragColor = inputPixelColor;
}
