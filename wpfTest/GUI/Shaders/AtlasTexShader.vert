﻿#version 420 core

in vec3 in_Position;
in vec3 in_Color;  
in vec2 in_TexCoord;
in vec4 in_TexLeftBottomWidthHeight;

out vec3 pass_Color;
out vec2 pass_TexCoord;
out vec4 pass_TexLeftBottomWidthHeight;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main(void) {
	gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(in_Position, 1.0);
	pass_Color = in_Color;
	pass_TexCoord = in_TexCoord;
	pass_TexLeftBottomWidthHeight = in_TexLeftBottomWidthHeight;
}