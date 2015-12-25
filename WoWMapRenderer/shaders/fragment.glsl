#version 330
 
out vec4 outputColor;
flat in int vert_type;

// Remember all code paths are always executed by GPU
void main()
{
    vec4 oColor = vec4(0.0f, 1.0f, 0.0f, 1.0f);
    if (vert_type == 1) // Doodad
        oColor = vec4(0.0f, 1.0f, 0.0f, 1.0f);
    else if (vert_type == 2) // WMO
        oColor = vec4(0.0f, 0.0f, 1.0f, 1.0f);
    outputColor = oColor;
}