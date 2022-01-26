#version 400 core
in vec2 position;

out vec4 clip_space;
out vec2 texture_coords;
out vec3 to_camera_vector;
out vec3 from_light_vector;
out float visibility;
out float pass_show_water;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;
uniform vec3 light_position;

uniform vec3 camera_position;

uniform float enable_fog;
uniform float fog_density;
uniform float fog_gradient;
uniform float show_water;

const float tiling = 4.0;

void main(void) {
	vec4 world_position = model_matrix * vec4(position.x, 0.0, position.y, 1.0);
	vec4 position_relative_to_cam = view_matrix * world_position;
	clip_space = projection_matrix * position_relative_to_cam;
	gl_Position = clip_space;
	texture_coords = vec2(position.x / 2.0 + 0.5, position.y / 2.0 + 0.5) * tiling;
	to_camera_vector = camera_position - world_position.xyz;
	from_light_vector = world_position.xyz - light_position;
	
	float distance = length(position_relative_to_cam.xyz);
	if (enable_fog > 0.5) {
		visibility = exp(-pow((distance * (fog_density)), fog_gradient)); // TODO: wasser hat zu viel schatten
		visibility = clamp(visibility, 0.0, 1.0);
	} else {
		visibility = 1;
	}
	pass_show_water = show_water;
}
