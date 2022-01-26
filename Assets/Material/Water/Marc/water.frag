#version 400 core
in vec4 clip_space;
in vec2 texture_coords;
in vec3 to_camera_vector;
in vec3 from_light_vector;
in float pass_show_water;

in float visibility;

layout (location = 0) out vec4 out_color;
layout (location = 1) out vec4 out_bright_color;

uniform sampler2D reflection_texture;
uniform sampler2D refraction_texture;
uniform sampler2D dudv_map;
uniform sampler2D normal_map;
uniform sampler2D depth_map;
uniform vec3 light_color;
uniform vec3 sky_color;

uniform float move_factor;

const float wave_strength = 0.04;
const float shine_damper = 20.0;
const float reflectivity = 0.5;

void main(void) {
	vec2 ndc = (clip_space.xy / clip_space.w) / 2.0 + 0.5;
	vec2 refraction_tex_coords = vec2(ndc.x, ndc.y);
	vec2 reflection_tex_coords = vec2(ndc.x, -ndc.y);

	float near = 0.1; // Must be the same values as in MasterRenderer!
	float far = 1500.0;  // Must be the same values as in MasterRenderer!
	float depth = texture(depth_map, refraction_tex_coords).r;
	float floor_distance = 2.0 * near * far / (far + near - (2.0 * depth - 1.0) * (far - near));

	depth = gl_FragCoord.z;
	float water_distance = 2.0 * near * far / (far + near - (2.0 * depth - 1.0) * (far - near));
	float water_depth = floor_distance - water_distance;

	vec2 distorted_tex_coords = texture(dudv_map, vec2(texture_coords.x + move_factor, texture_coords.y)).rg * 0.1;
	distorted_tex_coords = texture_coords + vec2(distorted_tex_coords.x, distorted_tex_coords.y + move_factor);
	vec2 total_distortion = (texture(dudv_map, distorted_tex_coords).rg * 2.0 - 1.0) * wave_strength * clamp(water_depth / 20.0, 0.0, 1.0);

	refraction_tex_coords += total_distortion;
	refraction_tex_coords = clamp(refraction_tex_coords, 0.001, 0.999);
	reflection_tex_coords += total_distortion;
	reflection_tex_coords.x = clamp(reflection_tex_coords.x, 0.001, 0.999);
	reflection_tex_coords.y = clamp(reflection_tex_coords.y, -0.999, -0.001);

	vec4 refract_color = texture(refraction_texture, refraction_tex_coords);
	vec4 reflect_color = texture(reflection_texture, reflection_tex_coords);

	vec4 normal_map_color = texture(normal_map, distorted_tex_coords);
	vec3 normal = vec3(normal_map_color.r * 2.0 - 1.0, normal_map_color.b * 3.0, normal_map_color.g * 2.0 - 1.0);
	normal = normalize(normal);

	vec3 view_vector = normalize(to_camera_vector);
	float refractive_factor = dot(view_vector, normal);
	//refractive_factor = pow(refractive_factor, 10.0); // increase to increase reflectivenesss
	
	vec3 reflected_light = reflect(normalize(from_light_vector), normal);
	float specular = max(dot(reflected_light, view_vector), 0.0);
	specular = pow(specular, shine_damper);
	vec3 specular_highlights = light_color * specular * reflectivity * clamp(water_depth / 5.0, 0.0, 1.0);
	out_color = mix(reflect_color, refract_color, refractive_factor);
	out_color = mix(out_color, vec4(0.0, 0.3, 0.5, 1.0), 0.2) + vec4(specular_highlights, 0.0); // give the water a blue tint
	out_color = out_color * vec4(light_color, 1.0); // Adjust brightness (at night)
	out_color = mix(vec4(sky_color, 1.0), out_color, visibility); // fog
	out_color.a = clamp(water_depth / 5.0, 0.0, pass_show_water);
	out_bright_color =  vec4(0.0);
}
