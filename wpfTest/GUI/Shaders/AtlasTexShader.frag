#version 330 core
in vec3 in_Position;
in vec3 pass_Color;

in vec2 pass_TexCoord;
in vec4 pass_TexLeftBottomWidthHeight;

out vec4 out_Color;

uniform sampler2D atlas0;
uniform sampler2D atlas1;

void main(void) {
	//v is a temporary variable used to confuse glsl so that it doesn't delete any of
	//the variables atlas0 and atlas1. It can be deleted once atlas index is passed to this shader
	vec2 v = vec2(0.5,in_Position[0]);
	if(v[0]>1)
		out_Color = texture(atlas0,vec2(pass_TexLeftBottomWidthHeight[0] //left coordinate
									+pass_TexCoord[0]*pass_TexLeftBottomWidthHeight[2],
									pass_TexLeftBottomWidthHeight[1]//bottom coordinate
									+pass_TexCoord[1]*pass_TexLeftBottomWidthHeight[3]))*vec4(pass_Color,1.0);
	else
		out_Color = texture(atlas1,vec2(pass_TexLeftBottomWidthHeight[0] //left coordinate
									+pass_TexCoord[0]*pass_TexLeftBottomWidthHeight[2],
									pass_TexLeftBottomWidthHeight[1]//bottom coordinate
									+pass_TexCoord[1]*pass_TexLeftBottomWidthHeight[3]))*vec4(pass_Color,1.0);
}