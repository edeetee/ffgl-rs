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
      "MAX": 1.0
    },
    {
      "NAME": "randomDeath",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1.0
    },
	{
      "NAME": "reproductionMin",
      "TYPE": "float",
      "DEFAULT": 2,
      "MIN": 0,
      "MAX": 8
    },
	{
      "NAME": "reproductionMax",
      "TYPE": "float",
      "DEFAULT": 4,
      "MIN": 0,
      "MAX": 8
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
	return (n.r + n.g + n.b) / 3.0f;
}

float hash12(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * .1031f);
	p3 += dot(p3, p3.yzx + 33.33f);
	return fract((p3.x + p3.y) * p3.z);
}

#define MAX_NEIGHBORS 8
#define MIN_NEIGHBORS 0

void main() {
	float outState = 0.0f;

	vec2 texc = isf_FragNormCoord.xy * RENDERSIZE;

	vec2 loc = texc;

	vec2 left_coord = vec2(texc.xy + vec2(-1.0f, 0));
	vec2 right_coord = vec2(texc.xy + vec2(1.0f, 0));
	vec2 above_coord = vec2(texc.xy + vec2(0, 1.0f));
	vec2 below_coord = vec2(texc.xy + vec2(0, -1.0f));

	vec2 lefta_coord = vec2(texc.xy + vec2(-1.0f, 1.0f));
	vec2 righta_coord = vec2(texc.xy + vec2(1.0f, 1.0f));
	vec2 leftb_coord = vec2(texc.xy + vec2(-1.0f, -1.0f));
	vec2 rightb_coord = vec2(texc.xy + vec2(1.0f, -1.0f));

	if((TIME < 0.5f) || (restartNow)) {
		//	randomize the start conditions
		float alive = hash12(vec2(TIME + 1.0f, 2.1f * TIME + 0.1f) + loc + vec2(1232));
		if(alive > 1.0f - startThresh) {
			outState = 1.0f;
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

		float underpopulation = clamp((reproductionMin - neighborSum) / (reproductionMin - MIN_NEIGHBORS), 0, 1);
		float overpopulation = clamp((neighborSum - reproductionMax) / (MAX_NEIGHBORS - reproductionMax), 0, 1);
		float reproduction = clamp((neighborSum - reproductionMin) / (reproductionMax - reproductionMin), 0, 1);

		float rand = hash12(vec2(TIME + 1.0f, 235.1f * TIME + 0.1f) + loc + vec2(0.02312f));

		float deathliness = 1 - max(underpopulation, overpopulation);
		// float birthliness = reproduction;

		// if hash12(vec2(TIME * 10.32123f + 123))
		// 	< randomDeath {
		// 	outState = 0.0;
		// }

		outState = clamp((state + hash12(vec2(TIME + 02.427f) + loc * 1.24f)) * deathliness, 0, 1);

		outState += hash12(vec2(TIME * 19.32123f + 0041.23f) + loc * 891.279f) * randomRegrowth;

		outState *= step(randomDeath, hash12(vec2(TIME * 10.32123f + 123) + loc * 166.3f));

	// 	//	live cell
	// 	if(state > 0.0f) {
	// 		if(rand < underpopulation) {
	// 			//	under population
	// 			inputPixelColor = vec4(0.0f);

	// 		//	status quo
	// 		} else if(neighborSum < reproductionMax) {
	// 			inputPixelColor = vec4(1.0f);

	// 			//	spontaneous death?
	// 			if(rand > 1.0f - randomDeath) {
	// 				inputPixelColor = vec4(0.0f);
	// 			}
	// 		} else if(rand < overpopulation) {
	// 			//	over population
	// 			inputPixelColor = vec4(0.0f);
	// 		}
	// 	}
	// 	//	dead cell
	// 	else {
	// 		if((neighborSum > reproductionMin) && (neighborSum < reproductionMax)) {
	// 			//	reproduction
	// 			inputPixelColor = vec4(1.0f);
	// 		} else if(neighborSum < reproductionMin) {
	// 			if(alive > 1.0f - randomRegrowth) {
	// 				inputPixelColor = vec4(1.0f);
	// 			}
	// 		}
	// 	}
	}

	gl_FragColor = vec4(outState);
}
