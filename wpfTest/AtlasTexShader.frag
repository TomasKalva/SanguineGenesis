#version 420 core
in vec3 in_Position;
in vec3 pass_Color;
in vec2 pass_TexCoord;
in vec2 pass_TexBottomLeft;

out vec4 out_Color;

uniform vec3 texExtents;
uniform vec3 atlasExtents;

uniform sampler2D ourTexture;

void main(void) {
	//out_Color = vec4(pass_Color, 1.0);
	//out_Color = texture(ourTexture,vec2(pass_TexCoord[0]/4,pass_TexCoord[1]/4));
	out_Color = texture(ourTexture,vec2(pass_TexBottomLeft[0]/atlasExtents[0] //left coordinate
									+pass_TexCoord[0]*texExtents[0]/atlasExtents[0],
									pass_TexBottomLeft[1]/atlasExtents[1]//bottom coordinate
									+pass_TexCoord[1]*texExtents[1]/atlasExtents[1]));
	/*out_Color = texture(ourTexture,vec2(pass_TexBottomLeft[0]/atlasExtents[0] //left coordinate
									+pass_TexCoord[0]/texExtents[0],
									pass_TexBottomLeft[1]/atlasExtents[1]//bottom coordinate
									+pass_TexCoord[1]/texExtents[1]));*/
}