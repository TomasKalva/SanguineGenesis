#version 420 core
in vec3 pass_Color;
in vec2 pass_TexCoord;

out vec4 out_Color;

uniform sampler2D ourTexture;

void main(void) {
	//out_Color = vec4(pass_Color, 1.0);
	out_Color = texture(ourTexture,pass_TexCoord);//*vec4(pass_Color,1.0);
}