#version 330

in vec3 vShading; /* color shader */
in vec2 texCoord; /* texture coordinates */

uniform sampler2D sampler; /* texture sampler */

out vec4 outputColor;

void main()
{
    outputColor = texture(sampler, texCoord) * vec4(vShading, 1.0f);
}