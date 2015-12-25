#version 330
 
in vec3 vPosition;
in int type;

uniform mat4 projection_modelview;
flat out int vert_type;

void main()
{
    gl_Position = projection_modelview * vec4(vPosition, 1.0f);
    vert_type = type;
}