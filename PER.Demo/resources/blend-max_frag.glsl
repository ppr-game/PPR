#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D target;
uniform sampler2D current;

void main(){
    vec4 targetTex = texture(target, TexCoords.st).rgba;
    vec4 currentTex = texture(current, TexCoords.st).rgba;

    vec4 result = vec4(max(targetTex.x, currentTex.x),
                       max(targetTex.y, currentTex.y),
                       max(targetTex.z, currentTex.z),
                       max(targetTex.w, currentTex.w));

    FragColor = result;
}
