#version 330

out vec4 outputColor;
in vec2 texCoord; // texture coordinates
uniform sampler2D sample; // texture sampler

// Remember all code paths are always executed by GPU
void main()
{
    outputColor = texture(sample, texCoord);
}