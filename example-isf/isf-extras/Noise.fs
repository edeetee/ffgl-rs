
/*{
	"CREDIT": "by VIDVOX",
	"ISFVSN": "2",
	"CATEGORIES": [
		"Noise"
	],
	"INPUTS": [
		{
			"NAME": "seed",
			"LABEL": "Random Seed",
			"TYPE": "float",
			"MIN": 0.01,
			"MAX": 1.0,
			"DEFAULT": 0.5
		},
		{
			"NAME": "cell_size",
			"LABEL": "Cell Size",
			"TYPE": "float",
			"MIN": 0.0,
			"MAX": 0.5,
			"DEFAULT": 0.125
		},
		{
			"NAME": "threshold",
			"LABEL": "Threshold",
			"TYPE": "float",
			"MIN": 0.0,
			"MAX": 1.0,
			"DEFAULT": 0.0
		},
		{
			"NAME": "use_time",
			"LABEL": "Animated",
			"TYPE": "bool",
			"DEFAULT": 1.0
		},
		{
			"NAME": "color_mode",
			"LABEL": "Color Mode",
			"TYPE": "long",
			"VALUES": [
				0,
				1,
				2,
				3
			],
			"LABELS": [
				"B&W",
				"Alpha",
				"RGB",
				"RGBA"
			],
			"DEFAULT": 2
		}
	]
}*/

#define ASPECT RENDERSIZE.x / RENDERSIZE.y

float rand(vec2 co) {
	return fract(sin(dot(co.xy, vec2(12.9898f, 78.233f))) * 43758.5453f);
}

void main() {
// CALCULATE EDGES OF CURRENT CELL
	float tmpSeed = seed;
	if(use_time) {
		tmpSeed = max(mod(tmpSeed * TIME, 1.0f), 0.01f);
	}

	//	if the size is 0.0 do this for every pixel
	if(cell_size == 0.0f) {
		vec4 outColor = vec4(0.0f);
		float translated = RENDERSIZE.x * isf_FragNormCoord[0] + isf_FragNormCoord[1];
		float val = rand(vec2(translated, tmpSeed));
		if(val >= threshold) {
			//	b&w
			if(color_mode == 0) {
				outColor = vec4(1.0f);
			}
			//	grayscale, use the alpha
			else if(color_mode == 1) {
				outColor = vec4(1.0f, 1.0f, 1.0f, val);
			}
			//	RGB
			else if(color_mode == 2) {
				float rRand = rand(vec2(translated + 0.1542f, tmpSeed));
				float gRand = rand(vec2(translated + 0.0835f, tmpSeed));
				float bRand = rand(vec2(translated + 0.2547f, tmpSeed));
				outColor = vec4(rRand, gRand, bRand, 1.0f);
			}
			//	RGBA
			else if(color_mode == 3) {
				float rRand = rand(vec2(translated + 0.1542f, tmpSeed));
				float gRand = rand(vec2(translated + 0.0835f, tmpSeed));
				float bRand = rand(vec2(translated + 0.2547f, tmpSeed));
				outColor = vec4(rRand, gRand, bRand, val);
			}
		}
		gl_FragColor = outColor;
	} else {
		// Position of current pixel
		vec2 xy;
		xy.x = isf_FragNormCoord[0];
		xy.y = isf_FragNormCoord[1];

		// Left and right of tile
		float CellWidth = cell_size;
		float CellHeight = cell_size * ASPECT;

		//	divide 1 by the cell width and cell height to determine the count
		float rows = floor(1.0f / CellHeight);
		float cols = floor(1.0f / CellWidth);
		float count = floor(rows * cols);

		//	figure out the ID # of the region
		float region = cols * floor(xy.x / CellWidth) + floor(xy.y / CellHeight);

		//	use this to draw the gradient of the regions as gray colors..
		//gl_FragColor = vec4(vec3(region/count),1.0);

		//	now translate this region to another random region using our seed and region
		float translated = clamp(rand(vec2(region / count, tmpSeed)), 0.0f, 1.0f);
		//translated = region/count;
		//gl_FragColor = vec4(vec3(translated),1.0);

		//	quantize the translated!
		translated = floor(count * translated);
		//gl_FragColor = vec4(vec3(translated),1.0);
		//	now convert the translated region back to an xy location
		//	get the relative position within the original block and then add on the translated amount
		xy.x = (xy.x - floor(xy.x / CellWidth) * CellWidth) + CellWidth * floor(translated / rows);
		//xy.x = (xy.x - floor(xy.x / CellWidth)*CellWidth);
		xy.y = xy.y - floor(xy.y / CellHeight) * CellHeight + CellHeight * floor(mod(translated, cols));

		float val = rand(vec2(translated, tmpSeed));

		vec4 outColor = vec4(0.0f);

		if(val >= threshold) {
			//	b&w
			if(color_mode == 0) {
				outColor = vec4(1.0f);
			}
			//	grayscale, use the alpha
			else if(color_mode == 1) {
				outColor = vec4(1.0f, 1.0f, 1.0f, val);
			}
			//	RGB
			else if(color_mode == 2) {
				float rRand = rand(vec2(translated + 0.1542f, tmpSeed));
				float gRand = rand(vec2(translated + 0.0835f, tmpSeed));
				float bRand = rand(vec2(translated + 0.2547f, tmpSeed));
				outColor = vec4(rRand, gRand, bRand, 1.0f);
			}
			//	RGBA
			else if(color_mode == 3) {
				float rRand = rand(vec2(translated + 0.1542f, tmpSeed));
				float gRand = rand(vec2(translated + 0.0835f, tmpSeed));
				float bRand = rand(vec2(translated + 0.2547f, tmpSeed));
				outColor = vec4(rRand, gRand, bRand, val);
			}
		}

		gl_FragColor = outColor;

	}
}