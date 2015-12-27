#version 330
 
in vec3 vertice_position; /* Input vertex */

uniform mat4 projection_modelview; /* projection * modelview */

in vec3 vertex_shading;
out vec3 vshading;
in vec2 in_TexCoord0; /* texture coordinates */
out vec2 texcoord;

void main()
{
	vshading = vertex_shading;
    texcoord = in_TexCoord0;
    gl_Position = projection_modelview * vec4(vertice_position, 1.0f);
}