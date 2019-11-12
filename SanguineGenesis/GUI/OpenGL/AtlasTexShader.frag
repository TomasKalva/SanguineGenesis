#version 330 core

in vec2 pass_TexCoord;					// uv coordinates
in vec4 pass_TexLeftBottomWidthHeight;	// coordinates in atlas

out vec4 out_Color;

uniform sampler2D atlas;

void main(void) {
	out_Color = texture(atlas,vec2(pass_TexLeftBottomWidthHeight[0]			//left coordinate
								+pass_TexCoord[0]*pass_TexLeftBottomWidthHeight[2],
								pass_TexLeftBottomWidthHeight[1]				//bottom coordinate
								+pass_TexCoord[1]*pass_TexLeftBottomWidthHeight[3]));
}