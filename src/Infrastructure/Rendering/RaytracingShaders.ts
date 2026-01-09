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
const int MAX_SPHERES = 32;
const int MAX_BOXES = 32;
const int MAX_PLANES = 8;

uniform int uNumLights;
uniform Light uLights[MAX_LIGHTS];

uniform int uNumSpheres;
uniform vec4 uSpheres[MAX_SPHERES * 2];

uniform int uNumBoxes;
uniform vec4 uBoxes[MAX_BOXES * 3];

uniform int uNumPlanes;
uniform vec4 uPlanes[MAX_PLANES * 3];

Hit intersectSphere(Ray ray, vec3 center, float radius, vec3 color) {
    Hit h; h.hit = false;
    vec3 oc = ray.origin - center;
    float b = dot(oc, ray.direction);
    float c = dot(oc, oc) - radius * radius;
    float d = b * b - c;
    if (d > 0.0) {
        float t = -b - sqrt(d);
        if (t > EPSILON) {
            h.hit = true; h.dist = t;
            h.point = ray.origin + ray.direction * t;
            h.normal = normalize(h.point - center);
            h.color = color;
        }
    }
    return h;
}

Hit intersectBox(Ray ray, vec3 bmin, vec3 bmax, vec3 color) {
    Hit h; h.hit = false;
    vec3 invD = 1.0 / ray.direction;
    vec3 t0 = (bmin - ray.origin) * invD;
    vec3 t1 = (bmax - ray.origin) * invD;
    vec3 tmin = min(t0, t1);
    vec3 tmax = max(t0, t1);
    float tn = max(max(tmin.x, tmin.y), tmin.z);
    float tf = min(min(tmax.x, tmax.y), tmax.z);
    if (tn < tf && tf > EPSILON) {
        float t = tn > EPSILON ? tn : tf;
        h.hit = true; h.dist = t;
        h.point = ray.origin + ray.direction * t;
        vec3 c = (bmin + bmax) * 0.5;
        vec3 p = h.point - c;
        vec3 d = (bmin - bmax) * 0.5;
        h.normal = normalize(sign(p) * step(abs(d.yzx), abs(p)) * step(abs(d.zxy), abs(p)));
        h.color = color;
    }
    return h;
}

Hit intersectPlane(Ray ray, vec3 p, vec3 n, vec3 color) {
    Hit h; h.hit = false;
    float d = dot(n, ray.direction);
    if (abs(d) > EPSILON) {
        float t = dot(p - ray.origin, n) / d;
        if (t > EPSILON) {
            h.hit = true; h.dist = t;
            h.point = ray.origin + ray.direction * t;
            h.normal = n;
            h.color = color;
        }
    }
    return h;
}

Hit trace(Ray ray) {
    Hit closest; closest.hit = false; closest.dist = MAX_DIST;
    
    for (int i = 0; i < MAX_SPHERES; i++) {
        if (i >= uNumSpheres) break;
        
        vec3 center = uSpheres[i * 2].xyz;
        float radius = uSpheres[i * 2].w;
        vec3 color = uSpheres[i * 2 + 1].rgb;
        
        Hit h = intersectSphere(ray, center, radius, color);
        if (h.hit && h.dist < closest.dist) closest = h;
    }
    
    for (int i = 0; i < MAX_BOXES; i++) {
        if (i >= uNumBoxes) break;
        
        vec3 bmin = uBoxes[i * 3].xyz;
        vec3 bmax = uBoxes[i * 3 + 1].xyz;
        vec3 color = uBoxes[i * 3 + 2].rgb;
        
        Hit h = intersectBox(ray, bmin, bmax, color);
        if (h.hit && h.dist < closest.dist) closest = h;
    }
    
    for (int i = 0; i < MAX_PLANES; i++) {
        if (i >= uNumPlanes) break;
        
        vec3 pos = uPlanes[i * 3].xyz;
        vec3 normal = uPlanes[i * 3 + 1].xyz;
        vec3 color = uPlanes[i * 3 + 2].rgb;
        
        Hit h = intersectPlane(ray, pos, normal, color);
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

