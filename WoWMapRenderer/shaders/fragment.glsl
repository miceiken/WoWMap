#version 330

flat in int vertex_type;

out vec4 outputColor; // R G B A

void main()
{
	vec4 color;
	if (vertex_type == 0) // Terrain
		color = vec4(0.0f, 1.0f, 0.0f, 1.0f);
	else if (vertex_type == 1) // WMO
		color = vec4(1.0f, 0.0f, 0.0f, 1.0f);
	else if (vertex_type == 2) // M2
		color = vec4(1.0f, 1.0f, 0.0f, 1.0f);
	else if (vertex_type == 3) // Water
		color = vec4(0.0f, 0.0f, 1.0f, 1.0f);
	else // Can't be set in first line because glsl is very weird at eval'ing branches
		color = vec4(1.0, 1.0f, 1.0f, 1.0f);
	
	outputColor = color;
}