#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D imageA;
uniform sampler2D imageB;

void main(){
    vec4 texA = texture(imageA, TexCoords.st).rgba;
    vec4 texB = texture(imageB, TexCoords.st).rgba;

    vec4 result = vec4(max(texA.x, texB.x), max(texA.y, texB.y), max(texA.z, texB.z), max(texA.w, texB.w));

    FragColor = result;
}
