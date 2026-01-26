export const vertexShaderSource = `
attribute vec3 aPosition;
attribute vec3 aNormal;
attribute vec4 aColor;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;
uniform vec4 uColor;

varying vec3 vNormal;
varying vec4 vColor;
varying vec3 vPosition;

void main() {
    vec4 worldPosition = uModelMatrix * vec4(aPosition, 1.0);
    vPosition = worldPosition.xyz;
    vNormal = normalize(mat3(uModelMatrix) * aNormal);
    vColor = aColor.a > 0.0 ? aColor : uColor;
    
    gl_Position = uProjectionMatrix * uViewMatrix * worldPosition;
}
`;

export const fragmentShaderSource = `
precision mediump float;

varying vec3 vNormal;
varying vec4 vColor;
varying vec3 vPosition;

uniform vec3 uLightDirection;
uniform vec3 uAmbientLight;

void main() {
    vec3 normal = normalize(vNormal);
    vec3 lightDir = normalize(uLightDirection);
    
    float diffuse = max(dot(normal, lightDir), 0.0);
    
    vec3 ambient = uAmbientLight * vColor.rgb;
    vec3 diffuseColor = diffuse * vColor.rgb * 0.8;
    
    vec3 finalColor = ambient + diffuseColor;
    
    gl_FragColor = vec4(finalColor, vColor.a);
}
`;

