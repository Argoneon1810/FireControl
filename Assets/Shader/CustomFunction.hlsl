uniform float _Points[6*4];
 
void Test_float(float3 position, out float3 direction, out float strength){
    float3 directionOutput = 0;
    float strengthOutput = 0;
     
    [unroll]
    for (int i = 0; i < 6*4; i += 4){
        float3 p = float3(_Points[i], _Points[i+1], _Points[i+2]); // Position
        float t = _Points[i+3]; // Lifetime
         
        // Ripple Shape :
        float rippleSize = 1;
        float gradient = smoothstep(t/3, t, distance(position, p) / rippleSize);
         
        // frac means it will have a sharp edge, while sine makes it more "soft"
        //float ripple = frac(gradient);
        float ripple = saturate(sin(5 * (gradient)));
         
        // Distortion Direction & Strength :
        float3 rippleDirection = normalize(position-p);
         
        float lifetimeFade = saturate(1-t); // Goes from 1 at t=0, to 0 at t=1
        float rippleStrength = lifetimeFade * ripple;
         
        directionOutput += rippleDirection * rippleStrength * 0.2;
        strengthOutput += rippleStrength;
    }
     
    direction = directionOutput;
    strength = strengthOutput;
}