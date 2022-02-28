#version 130
//layout (location = 0) in vec3 aPos;
//layout (location = 1) in vec2 aTexCoords;

out vec2 TexCoords;

void main() {
	//gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
	//gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
	//gl_FrontColor = gl_Color;
	gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
	TexCoords = vec2(gl_TextureMatrix[0] * gl_MultiTexCoord0);
	gl_FrontColor = gl_Color;
}
