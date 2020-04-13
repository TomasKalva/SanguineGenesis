#version 330 core

in vec3 in_Position;
in vec2 in_TexCoord;
in vec4 in_TexLeftBottomWidthHeight;

out vec2 pass_TexCoord;
out vec4 pass_TexLeftBottomWidthHeight;

uniform mat4 projectionMatrix;

void main(void) {
	gl_Position = projectionMatrix *  vec4(in_Position, 1.0);
	pass_TexCoord = in_TexCoord;
	pass_TexLeftBottomWidthHeight = in_TexLeftBottomWidthHeight;
}