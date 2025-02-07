using Domain.Enums;
namespace ApplicaionServiceInterface.Dtos.Requests
{
    /// <summary>
    /// �޸�Ȩ�����
    /// </summary>
    public class EditPermissionReq
    {
        /// <summary>
        /// �˵���
        /// </summary>
        public string MenuName { get; set; }
        /// <summary>
        /// ��id
        /// </summary>
        public int? Pid { get; set; }
        /// <summary>
        /// �˵�ҳ
        /// </summary>
        public string MenuPage { get; set; }
        /// <summary>
        /// �˵�����
        /// </summary>
        public PermissionMenuType MenuType { get; set; }
        /// <summary>
        /// icon
        /// </summary>
        public string MenuIcon { get; set; }
        /// <summary>
        /// ״̬
        /// </summary>
        public PermissionStatus Status { get; set; }
        /// <summary>
        /// id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// �˵�����
        /// </summary>
        public string MenuDisplayInfo { get; set; }
        /// <summary>
        /// �Ƿ���ϵͳ�˵�
        /// </summary>
        public bool ShowSystem { get; set; }
    }
}