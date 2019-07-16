#version 420 core
in vec3 in_Position;
in vec3 pass_Color;

in vec2 pass_TexCoord;
in vec4 pass_TexLeftBottomWidthHeight;

out vec4 out_Color;

uniform sampler2D atlas;

void main(void) {
	out_Color = texture(atlas,vec2(pass_TexLeftBottomWidthHeight[0] //left coordinate
									+pass_TexCoord[0]*pass_TexLeftBottomWidthHeight[2],
									pass_TexLeftBottomWidthHeight[1]//bottom coordinate
									+pass_TexCoord[1]*pass_TexLeftBottomWidthHeight[3]))*vec4(pass_Color,1.0);
}