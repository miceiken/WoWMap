#version 330

in vec3 vertexShading; /* color shader */
in vec2 fragment_in_texcoord; /* texture coordinates */

uniform sampler2D texture_sampler;

out vec4 outputColor;

void main()
{
    outputColor = texture(texture_sampler, fragment_in_texcoord) * vec4(vertexShading, 1.0f);
}