#version 330
 
in vec3 vPosition; /* Input vertex */
in vec2 in_TexCoord0; /* texture coordinates, forwarded to fragment shader */
in vec2 vertexShading; /* vertex shader, forwarded to fragment shader */

uniform mat4 projection_modelview; /* projection * modelview */

out vec2 texCoord; /* texture coordinates, in for fragment shader */
out vec2 vShading;

void main()
{
    texCoord = in_TexCoord0;
    vShading = vertexShading;
    gl_Position = projection_modelview * vec4(vPosition, 1.0f);
}