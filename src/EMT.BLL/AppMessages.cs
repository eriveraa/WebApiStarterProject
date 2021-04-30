namespace EMT.BLL
{

    public class AppMessages
    {
        // E001
        public const string RECORDNOTFOUND = "No se encontró el registro solicitado.";

        // E002
        public const string USERNOTFOUND = "Usuario o contraseña incorrectos. Por favor ingrese unas credenciales válidas.";

        // E003
        public const string USERADNOTFOUND = "Usuario NO encontrado o desactivado. Por favor contacte a su administrador de sistemas.";

        // E004
        public const string USERALREADYEXISTING = "Se encontró un Usuario con el mismo email. Por favor utilice otro email.";

        // E005
        public const string INVALIDDATA = "Los datos ingresados no son válidos. Por favor corregir.";

        // E006
        public const string LOGICERROR = "Error en lógica de negocio.";

        public const string UPDATE_ID_ERROR = "Inconsistencia entre el ID y el objeto.";
        public const string UPDATE_FORBIDDEN_FIELDS = "Detectado cambios en campos no autorizados.";
        public const string CREATE_EXISTING_ID = "Ya existe un registro con ese ID.";

    }
}
