using System.Collections.ObjectModel;
using RobotHri.Languages;
using RobotHri.Services;

namespace RobotHri.ViewModels;

public class ProcedureListViewModel : BaseViewModel
{
    private string _titleText = string.Empty;
    private string _homeText = string.Empty;
    private string _languageLabel = "VI";
    private string _listTitleText = string.Empty;
    private string _subtitleText = string.Empty;
    private string _colSttText = string.Empty;
    private string _colNameText = string.Empty;
    private string _colCounterText = string.Empty;
    private string _colDocsText = string.Empty;
    private string _colNoteText = string.Empty;

    public ObservableCollection<ProcedureTableRow> Rows { get; } = new();

    public string TitleText { get => _titleText; set => SetProperty(ref _titleText, value); }
    public string HomeText { get => _homeText; set => SetProperty(ref _homeText, value); }
    public string LanguageLabel { get => _languageLabel; set => SetProperty(ref _languageLabel, value); }
    public string ListTitleText { get => _listTitleText; set => SetProperty(ref _listTitleText, value); }
    public string SubtitleText { get => _subtitleText; set => SetProperty(ref _subtitleText, value); }
    public string ColSttText { get => _colSttText; set => SetProperty(ref _colSttText, value); }
    public string ColNameText { get => _colNameText; set => SetProperty(ref _colNameText, value); }
    public string ColCounterText { get => _colCounterText; set => SetProperty(ref _colCounterText, value); }
    public string ColDocsText { get => _colDocsText; set => SetProperty(ref _colDocsText, value); }
    public string ColNoteText { get => _colNoteText; set => SetProperty(ref _colNoteText, value); }

    public Command GoHomeCommand { get; }
    public Command ToggleLanguageCommand { get; }

    public ProcedureListViewModel(ILocalizationService localization) : base(localization)
    {
        GoHomeCommand = new Command(async () => await Shell.Current.GoToAsync("//main"));
        ToggleLanguageCommand = new Command(Localization.ToggleLanguage);
        RefreshLocalizedProperties();
    }

    protected override void RefreshLocalizedProperties()
    {
        TitleText = StringIds.PROCEDURE_TITLE.GetString();
        HomeText = StringIds.COMMON_HOME.GetString();
        LanguageLabel = Localization.GetCurrentLanguageName();
        ListTitleText = StringIds.PROCEDURE_SUPPORTED_LIST_TITLE.GetString();
        SubtitleText = StringIds.PROCEDURE_PROMPT.GetString();
        ColSttText = StringIds.PROCEDURE_COL_STT.GetString();
        ColNameText = StringIds.PROCEDURE_COL_NAME.GetString();
        ColCounterText = StringIds.PROCEDURE_COL_COUNTER.GetString();
        ColDocsText = StringIds.PROCEDURE_COL_DOCS.GetString();
        ColNoteText = StringIds.PROCEDURE_COL_NOTE.GetString();

        Rows.Clear();

        if (Localization.CurrentLanguageCode == "vi")
        {
            Rows.Add(new ProcedureTableRow(
                "1",
                "Chứng thực bản sao từ bản chính",
                "Quầy 1: CHỨNG THỰC BẢN SAO",
                "- Bản chính giấy tờ, văn bản.\n- Bản sao (photo) sẵn hoặc yêu cầu sao chụp tại chỗ.",
                "Bản chính phải còn nguyên vẹn, không có dấu hiệu tẩy xóa hay sửa chữa trái pháp luật."));
            Rows.Add(new ProcedureTableRow(
                "2",
                "Chứng thực chữ ký trong giấy tờ, văn bản",
                "Quầy 2: CHỨNG THỰC CHỮ KÝ",
                "- Bản gốc CCCD/Hộ chiếu còn giá trị.\n- Giấy tờ, văn bản cần ký.",
                "Người yêu cầu tuyệt đối không ký trước; phải thực hiện ký trực tiếp trước mặt cán bộ tiếp nhận."));
            Rows.Add(new ProcedureTableRow(
                "3",
                "Trả kết quả các thủ tục hành chính",
                "Quầy 3: TRẢ KẾT QUẢ HỒ SƠ HÀNH CHÍNH",
                "- Giấy hẹn trả kết quả (bản gốc).\n- Bản chính CCCD người nộp.\n- Biên lai nộp phí (nếu có).",
                "Nếu nộp hồ sơ trực tuyến, cần mang theo bản chính đối chiếu nếu có yêu cầu trong giấy hẹn."));
            Rows.Add(new ProcedureTableRow(
                "4",
                "Đăng ký khai sinh/khai tử có yếu tố nước ngoài",
                "Quầy 4: TƯ PHÁP - HỘ TỊCH",
                "- Giấy chứng sinh/Giấy báo tử.\n- Tờ khai/giấy tờ có giá trị thay thế của người nước ngoài.",
                "Cần lưu ý về ngôn ngữ của giấy tờ nước ngoài (phải dịch thuật công chứng)."));
            Rows.Add(new ProcedureTableRow(
                "5",
                "Tiếp nhận đơn thư khiếu nại, tố cáo",
                "Quầy 5: TRẢ KẾT QUẢ TƯ PHÁP - HỘ TỊCH - TIẾP NHẬN ĐƠN THƯ",
                "- Đơn khiếu nại/tố cáo (ghi rõ nội dung).\n- Tài liệu, bằng chứng chứng minh.",
                "Đơn thư phải có thông tin liên hệ rõ ràng; đơn nặc danh sẽ không được xem xét."));
            Rows.Add(new ProcedureTableRow(
                "6",
                "Cấp giấy phép kinh doanh dịch vụ Karaoke",
                "Quầy 6: VĂN HÓA - XÃ HỘI",
                "- Đơn đề nghị cấp giấy phép.\n- Bản sao Giấy chứng nhận đăng ký doanh nghiệp.\n- Biên bản kiểm tra an toàn PCCC.",
                "Phải đảm bảo quy định về âm thanh và các điều kiện kinh doanh đặc thù."));
            Rows.Add(new ProcedureTableRow(
                "7",
                "Đăng ký kế hoạch bảo vệ môi trường",
                "Quầy 7: KINH TẾ - MÔI TRƯỜNG",
                "- Hồ sơ kế hoạch bảo vệ môi trường.\n- Báo cáo hiện trạng và mô tả dự án.\n- Bản sao Giấy chứng nhận đầu tư.",
                "Thủ tục này chỉ áp dụng cho các dự án chưa đến mức phải lập báo cáo ĐTM."));
            Rows.Add(new ProcedureTableRow(
                "8",
                "Cấp giấy phép xây dựng nhà ở riêng lẻ",
                "Quầy 8: ĐẤT ĐAI - XÂY DỰNG - HẠ TẦNG ĐÔ THỊ",
                "- Đơn đề nghị cấp phép xây dựng.\n- Giấy tờ chứng minh quyền sử dụng đất.\n- 02 bộ bản vẽ thiết kế xây dựng.",
                "Bản vẽ phải do đơn vị có tư cách pháp nhân thiết kế theo quy chuẩn xây dựng."));
            Rows.Add(new ProcedureTableRow(
                "9",
                "Cấp mới/đổi thẻ Bảo hiểm y tế",
                "Quầy 9: BẢO HIỂM Y TẾ - BẢO HIỂM XÃ HỘI",
                "- Tờ khai tham gia (Mẫu TK1-TS).\n- Bản sao CCCD.\n- Thẻ cũ (nếu là thủ tục đổi thẻ).",
                "Có thể thực hiện tra cứu mã số BHXH trước trên ứng dụng VSSID để làm thủ tục nhanh hơn."));
            return;
        }

        Rows.Add(new ProcedureTableRow(
            "1",
            "Copy certification from original",
            "Counter 1: COPY CERTIFICATION",
            "- Original document.\n- Copy (photo) or request on-site copy.",
            "Original documents must be intact and free from erasure signs."));
        Rows.Add(new ProcedureTableRow(
            "2",
            "Signature certification in documents",
            "Counter 2: SIGNATURE CERTIFICATION",
            "- Valid ID card/passport.\n- Documents requiring signature.",
            "Requester must not sign in advance; signature must be made before the receiving officer."));
        Rows.Add(new ProcedureTableRow(
            "3",
            "Administrative result return",
            "Counter 3: ADMINISTRATIVE RESULT RETURN",
            "- Original appointment slip.\n- Original ID of submitter.\n- Fee receipt (if any).",
            "For online dossiers, bring originals for comparison when requested."));
        Rows.Add(new ProcedureTableRow(
            "4",
            "Birth/death registration with foreign elements",
            "Counter 4: JUSTICE - CIVIL STATUS",
            "- Birth certificate/death notice.\n- Required declaration/alternative legal documents.",
            "Foreign-language documents may require notarized translation."));
        Rows.Add(new ProcedureTableRow(
            "5",
            "Receive complaint and denunciation petitions",
            "Counter 5: JUDICIAL RESULT - PETITION RECEIVING",
            "- Complaint/denunciation form with clear content.\n- Supporting evidence.",
            "Contact information must be clear; anonymous petitions may be rejected."));
        Rows.Add(new ProcedureTableRow(
            "6",
            "Business license for karaoke service",
            "Counter 6: CULTURE - SOCIETY",
            "- License request form.\n- Business registration copy.\n- Fire safety inspection minutes.",
            "Must comply with sound regulations and sector-specific business conditions."));
        Rows.Add(new ProcedureTableRow(
            "7",
            "Environmental protection plan registration",
            "Counter 7: ECONOMY - ENVIRONMENT",
            "- Environmental protection plan dossier.\n- Current status report and project description.\n- Investment certificate copy.",
            "Applied to projects that do not yet require a full EIA report."));
        Rows.Add(new ProcedureTableRow(
            "8",
            "Construction permit for private house",
            "Counter 8: LAND - CONSTRUCTION - URBAN INFRASTRUCTURE",
            "- Construction permit application.\n- Land-use rights documents.\n- Two sets of construction design drawings.",
            "Drawings must be prepared by legally qualified design units."));
        Rows.Add(new ProcedureTableRow(
            "9",
            "Issue/renew health insurance card",
            "Counter 9: HEALTH AND SOCIAL INSURANCE",
            "- Participation declaration (Form TK1-TS).\n- ID copy.\n- Old card (for renewal).",
            "You can check your social insurance code on VSSID first for faster processing."));
    }
}

public class ProcedureTableRow
{
    public string Index { get; }
    public string ProcedureName { get; }
    public string Counter { get; }
    public string RequiredDocs { get; }
    public string Note { get; }

    public ProcedureTableRow(string index, string procedureName, string counter, string requiredDocs, string note)
    {
        Index = index;
        ProcedureName = procedureName;
        Counter = counter;
        RequiredDocs = requiredDocs;
        Note = note;
    }
}
