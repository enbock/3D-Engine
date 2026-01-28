export const raytracingVertexShader = `
attribute vec2 aPosition;
varying vec2 vUV;
void main() {
    vUV = aPosition * 0.5 + 0.5;
    gl_Position = vec4(aPosition, 0.0, 1.0);
}
`;

export const raytracingFragmentShader = `
precision highp float;
varying vec2 vUV;
uniform vec2 uResolution;
uniform vec3 uCameraPosition;
uniform vec3 uCameraTarget;

struct Ray { vec3 origin; vec3 direction; };
struct Hit { bool hit; float dist; vec3 point; vec3 normal; vec3 color; };
struct Light { int type; vec3 position; vec3 direction; vec3 color; float intensity; };

const float EPSILON = 0.001;
const float MAX_DIST = 100.0;
const int MAX_LIGHTS = 8;

uniform int uNumLights;
uniform Light uLights[MAX_LIGHTS];

uniform int uNumTriangles;
uniform sampler2D uTriangleData;
uniform vec2 uTriangleTexSize;

vec4 getTriangleData(int index) {
    float x = mod(float(index), uTriangleTexSize.x);
    float y = floor(float(index) / uTriangleTexSize.x);
    vec2 uv = (vec2(x, y) + 0.5) / uTriangleTexSize;
    return texture2D(uTriangleData, uv);
}

Hit intersectTriangle(Ray ray, vec3 v0, vec3 v1, vec3 v2, vec3 color) {
    Hit h; h.hit = false;
    
    vec3 e1 = v1 - v0;
    vec3 e2 = v2 - v0;
    vec3 pvec = cross(ray.direction, e2);
    float det = dot(e1, pvec);
    
    if (abs(det) < EPSILON) return h;
    
    float invDet = 1.0 / det;
    vec3 tvec = ray.origin - v0;
    float u = dot(tvec, pvec) * invDet;
    
    if (u < 0.0 || u > 1.0) return h;
    
    vec3 qvec = cross(tvec, e1);
    float v = dot(ray.direction, qvec) * invDet;
    
    if (v < 0.0 || u + v > 1.0) return h;
    
    float t = dot(e2, qvec) * invDet;
    
    if (t > EPSILON) {
        h.hit = true;
        h.dist = t;
        h.point = ray.origin + ray.direction * t;
        h.normal = normalize(cross(e1, e2));
        h.color = color;
    }
    
    return h;
}

Hit trace(Ray ray) {
    Hit closest; closest.hit = false; closest.dist = MAX_DIST;
    
    for (int i = 0; i < 4096; i++) {
        if (i >= uNumTriangles) break;
        
        vec4 data0 = getTriangleData(i * 3);
        vec4 data1 = getTriangleData(i * 3 + 1);
        vec4 data2 = getTriangleData(i * 3 + 2);
        
        vec3 v0 = data0.xyz;
        vec3 v1 = data1.xyz;
        vec3 v2 = data2.xyz;
        vec3 color = vec3(data0.w, data1.w, data2.w);
        
        Hit h = intersectTriangle(ray, v0, v1, v2, color);
        if (h.hit && h.dist < closest.dist) closest = h;
    }
    
    return closest;
}

vec3 shade(Hit hit, vec3 rayDir) {
    vec3 ambient = vec3(0.0);
    vec3 diffuse = vec3(0.0);
    vec3 specular = vec3(0.0);
    
    for (int i = 0; i < MAX_LIGHTS; i++) {
        if (i >= uNumLights) break;
        
        Light light = uLights[i];
        
        if (light.type == 2) {
            ambient += light.color * light.intensity * hit.color;
        } else if (light.type == 0) {
            vec3 lightDir = normalize(light.direction);
            
            Ray shadow;
            shadow.origin = hit.point + hit.normal * EPSILON * 10.0;
            shadow.direction = lightDir;
            Hit sh = trace(shadow);
            float shad = sh.hit ? 0.3 : 1.0;
            
            float diff = max(dot(hit.normal, lightDir), 0.0);
            diffuse += diff * hit.color * light.color * light.intensity * shad;
            
            vec3 h = normalize(lightDir - rayDir);
            float spec = pow(max(dot(hit.normal, h), 0.0), 32.0);
            specular += spec * light.color * light.intensity * 0.5 * shad;
        } else if (light.type == 1) {
            vec3 lightDir = normalize(light.position - hit.point);
            float distance = length(light.position - hit.point);
            float attenuation = 1.0 / (1.0 + 0.1 * distance + 0.01 * distance * distance);
            
            Ray shadow;
            shadow.origin = hit.point + hit.normal * EPSILON * 10.0;
            shadow.direction = lightDir;
            Hit sh = trace(shadow);
            float shad = (sh.hit && sh.dist < distance) ? 0.3 : 1.0;
            
            float diff = max(dot(hit.normal, lightDir), 0.0);
            diffuse += diff * hit.color * light.color * light.intensity * attenuation * shad;
            
            vec3 h = normalize(lightDir - rayDir);
            float spec = pow(max(dot(hit.normal, h), 0.0), 32.0);
            specular += spec * light.color * light.intensity * 0.5 * attenuation * shad;
        }
    }
    
    return ambient + diffuse + specular;
}

void main() {
    vec2 uv = vUV * 2.0 - 1.0;
    uv.x *= uResolution.x / uResolution.y;
    
    vec3 forward = normalize(uCameraTarget - uCameraPosition);
    vec3 right = normalize(cross(forward, vec3(0, 1, 0)));
    vec3 up = cross(right, forward);
    
    float fov = 0.785398;
    vec3 rayDir = normalize(forward + right * uv.x * tan(fov * 0.5) + up * uv.y * tan(fov * 0.5));
    
    Ray ray; ray.origin = uCameraPosition; ray.direction = rayDir;
    Hit hit = trace(ray);
    
    vec3 color = vec3(0.1, 0.1, 0.15);
    if (hit.hit) {
        color = shade(hit, rayDir);
        vec3 refl = reflect(rayDir, hit.normal);
        ray.origin = hit.point + hit.normal * EPSILON * 10.0;
        ray.direction = refl;
        Hit hit2 = trace(ray);
        if (hit2.hit) {
            color += shade(hit2, refl) * 0.3;
        }
    } else {
        color = mix(vec3(0.1, 0.1, 0.15), vec3(0.5, 0.7, 1.0), rayDir.y * 0.5 + 0.5);
    }
    
    color = pow(color, vec3(0.4545));
    gl_FragColor = vec4(color, 1.0);
}
`;

