#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D imageA;
uniform sampler2D imageB;

void main(){
    vec4 texA = texture(imageA, TexCoords.st).rgba;
    vec4 texB = texture(imageB, TexCoords.st).rgba;

    vec4 result = vec4(min(texA.x, texB.x), min(texA.y, texB.y), min(texA.z, texB.z), min(texA.w, texB.w));

    FragColor = result;
}
