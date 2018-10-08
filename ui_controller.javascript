var moveSpeed  = 10;
var turnSpeed  = 50;
//var respawn : UnityEngine.GameObject;
var respawn = new UnityEngine.GameObject.ctor();

jss.define_mb("UIController", function() {
    
   
    respawn = UnityEngine.GameObject.FindWithTag ("Cube");
    respawn.SetActive(false);
    //respawn.UnityEngine.SetActive(false);
    
  /*  if (UnityEngine.Input.GetKeyDown$$KeyCode(UnityEngine.KeyCode.UpArrow)) {
    
    break;
    }

     if(UnityEngine.Input.GetKey(KeyCode.UpArrow))
    
    respawn.transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    
    if(UnityEngine.Input.GetKey(KeyCode.DownArrow))
        respawn.transform.Translate(-Vector3.forward * moveSpeed * Time.deltaTime);
    
    if(UnityEngine.Input.GetKey(KeyCode.LeftArrow))
        respawn.transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);
    
    if(UnityEngine.Input.GetKey(KeyCode.RightArrow))
        respawn.transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime); */

});