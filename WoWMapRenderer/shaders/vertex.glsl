#version 330
 
in vec3 vertice_position; /* Input vertex */
in vec2 in_TexCoord0; /* texture coordinates, forwarded to fragment shader */
in vec3 vertex_shading;

uniform mat4 projection_modelview; /* projection * modelview */

out vec2 fragment_in_texcoord; /* texture coordinates, in for fragment shader */
out vec3 vertexShading;
void main()
{
	vertexShading = vertex_shading;
    fragment_in_texcoord = in_TexCoord0;
    gl_Position = projection_modelview * vec4(vertice_position, 1.0f);
}